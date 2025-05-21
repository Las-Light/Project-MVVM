using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using UnityEngine;
using UnityEngine.AI;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem
{
    public abstract class SearchManager : MonoBehaviour
    {
        [SerializeField] private LayerMask hidableLayers;
        [SerializeField] private LayerMask obstacleMask;

        private float _maxSensitivity;
        private float _minSensitivity;
        private float _minTargetDistance;
        private float _maxTargetDistance;
        private float _minObstacleHeight;
        private float _maxObstacleHeight;
        private float _searchRadiusAreas;

        protected NavMeshAgent agent;
        protected EnemyData data;
        protected EnemyWorldData worldData;
        protected PatrolManager patrolManager;
        protected CombatController combatController;

        protected Collider[]
            colliders = new Collider[300]; // если каких-то укрытий не находит надо увеличить количество коллайдеров

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            data = GetComponent<EnemyData>();
            worldData = GetComponent<EnemyWorldData>();
            patrolManager = GetComponent<PatrolManager>();
            combatController = FindObjectOfType<CombatController>(); // установить через фабрику
        }


        // сканирование местности на наличие точек для укрытия или расследования
        protected void CheckAreaAndFindPoints(Vector3 targetPosition)
        {
            if (!data.CurrentEnemy && data.HeardSoundPosition == Vector3.zero)
                return;

            data.FoundCoverPos.Clear();
            data.FoundInvestigatePoint.Clear();

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(transform.position, _searchRadiusAreas, colliders,
                hidableLayers);

            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (worldData.IsTargetLost || worldData.IsCouldSeeEnemy || worldData.IsNeedCover)
                {
                    // проверка найденных точек на соответсвие условиям поиска по дальности от цели и высоте укрытия
                    if (Vector3.Distance(colliders[i].transform.position, targetPosition) < _minTargetDistance ||
                        Vector3.Distance(colliders[i].transform.position, data.CurrentEnemy.transform.position) >
                        _maxTargetDistance ||
                        colliders[i].bounds.size.y < _minObstacleHeight ||
                        colliders[i].bounds.size.y > _maxObstacleHeight)
                    {
                        colliders[i] = null;
                        hitReduction++;
                    }
                }

                if (worldData.IsHearingSound && !worldData.IsTargetLost && !worldData.IsNeedCover)
                {
                    if (Vector3.Distance(colliders[i].transform.position, targetPosition) < _minTargetDistance ||
                        Vector3.Distance(colliders[i].transform.position, targetPosition) > _maxTargetDistance ||
                        colliders[i].bounds.size.y < _minObstacleHeight ||
                        colliders[i].bounds.size.y > _maxObstacleHeight)
                    {
                        colliders[i] = null;
                        hitReduction++;
                    }
                }
            }

            hits -= hitReduction;

            // сортировка массива по дистанции от игрока
            System.Array.Sort(colliders, ColliderArraySortComparer);

            for (int i = 0; i < hits; i++)
            {
                // если на навмеше есть такая точка (позиция найденного колайдера), то он возвращает эту точку как навмешхит
                if (NavMesh.SamplePosition(colliders[i].transform.position, out NavMeshHit hit, 2f,
                        agent.areaMask))
                {
                    // ищет близжайшую грань навмеша к точке указанной выше и возвращает уже навмешхит этой грани
                    if (!NavMesh.FindClosestEdge(hit.position, out hit, agent.areaMask))
                    {
                        //Debug.LogError($"Unable to find edge close to {hit.position}");
                    }

                    // сравнивает нормаль навмешхита грани и направления навмешхита к цели, если дот меньше заданного sensitivity
                    // то записывает эту точку как Points в список FoundCoverPos
                    if (DotHitPoint(hit, targetPosition) < _maxSensitivity &&
                        DotHitPoint(hit, targetPosition) > _minSensitivity)
                    {
                        if (worldData.IsNeedCover)
                        {
                            var coverPoint = new Points
                            {
                                PointPosition = hit.position,
                                Height = colliders[i].bounds.size.z,
                                ShootPos = new Vector3(hit.position.x, 1.634f, hit.position.z)
                            };
                            if (!Physics.Raycast(coverPoint.ShootPos,
                                    (data.TargetAimPos - coverPoint.ShootPos).normalized,
                                    (coverPoint.ShootPos - data.TargetAimPos).magnitude, obstacleMask))
                            {
                                data.FoundCoverPos.Add(coverPoint);
                            }
                        }

                        if (worldData.IsNeedInvestigation)
                        {
                            var coverPoint = new Points
                            {
                                PointPosition = hit.position,
                                Height = colliders[i].bounds.size.z
                            };
                            data.FoundInvestigatePoint.Add(coverPoint);
                        }
                    }
                    // проверят еще точки (позиция колайдера + нормализованный вектор от цели к позиции колайдера)  
                    else if (NavMesh.SamplePosition(
                                 colliders[i].transform.position +
                                 (colliders[i].transform.position - targetPosition).normalized,
                                 out NavMeshHit hit2, 2f, agent.areaMask))
                    {
                        if (!NavMesh.FindClosestEdge(hit2.position, out hit2, agent.areaMask))
                        {
                            //Debug.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                        }

                        // сравнение нормали точки укрытия к вектору направленному от точки укрытия к цели.
                        // Отрицательные значения: на противоположной стороне от цели
                        // Положительные значения: на тойже стороне от цели
                        if (DotHitPoint(hit2, targetPosition) < _maxSensitivity &&
                            DotHitPoint(hit2, targetPosition) > _minSensitivity)
                        {
                            if (worldData.IsNeedCover)
                            {
                                var coverPoint = new Points
                                {
                                    PointPosition = hit2.position,
                                    Height = colliders[i].bounds.size.z,
                                    ShootPos = new Vector3(hit2.position.x, 1.634f, hit2.position.z)
                                };
                                if (!Physics.Raycast(coverPoint.ShootPos,
                                        (data.TargetAimPos - coverPoint.ShootPos).normalized,
                                        (coverPoint.ShootPos - data.TargetAimPos).magnitude, obstacleMask))
                                {
                                    data.FoundCoverPos.Add(coverPoint);
                                }
                            }

                            if (worldData.IsNeedInvestigation)
                            {
                                var coverPoint = new Points
                                {
                                    PointPosition = hit2.position,
                                    Height = colliders[i].bounds.size.z
                                };
                                data.FoundInvestigatePoint.Add(coverPoint);
                            }
                        }
                    }
                }
            }

            // создание точек для укрытий
            if (worldData.IsNeedCover)
            {
                if (data.FoundCoverPos.Count > 0)
                {
                    // проверка не заняты ли укрытия другими юнитами, если да то удаляет эти точки из FoundCoverPos
                    combatController.CheckCoverPointsForOccupied(data, gameObject);
                    if (data.FoundCoverPos.Count > 0)
                    {
                        // создание патрульных точек из найденных укрытий и установка текущей патрульной точки как текущего 
                        // укрытия, установка флага на начало движения к укрытию
                        if (patrolManager.CreateRouteUseSpecifiedPoints(data.FoundCoverPos.Count, data.FoundCoverPos))
                        {
                            if (patrolManager.CurrentPatrolPoint.DistToPoint < data.SqrtDistanceToTarget)
                            {
                                data.CurrentCover = patrolManager.CurrentPatrolPoint;
                                worldData.IsCoverPointPassed = false;
                                worldData.IsFindCover = true;
                            }
                        }
                    }
                }
            }

            // создание точек для расследования подозрительных локаций
            if (worldData.IsNeedInvestigation)
            {
                Debug.Log("StartSearch " + gameObject.name);
                if (data.FoundInvestigatePoint.Count > 0)
                {
                    // проверка найденных точек: не расследует ли кто другой их, если да то удаляет их из списка FoundInvestigatePoints
                    combatController.CheckInvestPointForOccupied(data, gameObject);
                    if (data.FoundInvestigatePoint.Count > 0)
                    {
                        // устанавливает флаг для начала расследования и переводит найденные точки в точки патрулирования 
                        if (patrolManager.CreateRouteUseSpecifiedPoints(data.FoundInvestigatePoint.Count,
                                data.FoundInvestigatePoint))
                        {
                            Debug.Log("Invest Spec " + gameObject.name);
                            worldData.IsFindInvestPoint = true;
                        }
                        else
                        {
                            Debug.Log("Invest random " + gameObject.name);
                            if (worldData.IsHearingSound && data.SoundDetectionTime > data.TargetLastKnownDetectionTime)
                            {
                                patrolManager.CreateRouteUseRandomPoints(
                                    data.HeardSoundPosition, 6, 10);
                            }
                            else if (worldData.IsTargetLost && data.TargetLastKnownPosition != Vector3.zero)
                                patrolManager.CreateRouteUseRandomPoints(
                                    data.TargetLastKnownPosition + data.TargetLastKnownDirection * 5, 6, 10);
                            else
                            {
                                Debug.LogError("NOT FOUND, NOT TARGET, NOT HEAR");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Found < 0 " + gameObject.name);
                        // если все точки заняты то начинает случайное расследование в зоне где последний раз видел врага или слышал звук
                        worldData.IsFindInvestPoint = true;
                        if (worldData.IsHearingSound && data.SoundDetectionTime > data.TargetLastKnownDetectionTime)
                        {
                            patrolManager.CreateRouteUseRandomPoints(
                                data.HeardSoundPosition, 6, 10);
                        }
                        else if (worldData.IsTargetLost && data.TargetLastKnownPosition != Vector3.zero)
                            patrolManager.CreateRouteUseRandomPoints(
                                data.TargetLastKnownPosition + data.TargetLastKnownDirection * 5, 6, 10);
                        else
                        {
                            Debug.LogError("NOT FOUND, NOT TARGET, NOT HEAR");
                        }
                    }
                }
                else
                {
                    Debug.Log("Found < 0 " + gameObject.name);
                    // если не нашел точек поблизости то начинает случайное расследование в зоне где последний раз видел врага или слышал звук
                    worldData.IsFindInvestPoint = true;
                    if (worldData.IsHearingSound && data.SoundDetectionTime > data.TargetLastKnownDetectionTime)
                    {
                        patrolManager.CreateRouteUseRandomPoints(
                            data.HeardSoundPosition, 6, 10);
                    }
                    else if (worldData.IsTargetLost && data.TargetLastKnownPosition != Vector3.zero)
                        patrolManager.CreateRouteUseRandomPoints(
                            data.TargetLastKnownPosition + data.TargetLastKnownDirection * 5, 6, 10);
                }
            }
        }

        protected void SettingsCheckArea(float minSens, float maxSens, float minDist, float maxDist, float minHeight,
            float maxHeight, float radius)
        {
            _minSensitivity = minSens;
            _maxSensitivity = maxSens;
            _minTargetDistance = minDist;
            _maxTargetDistance = maxDist;
            _minObstacleHeight = minHeight;
            _maxObstacleHeight = maxHeight;
            _searchRadiusAreas = radius;
        }

        // проверка нормали hit к позиции цели возвращает значение с -1 если hit находится на противоположной стороне от цели
        protected float DotHitPoint(NavMeshHit hit, Vector3 targetPosition)
        {
            return Vector3.Dot(hit.normal, (targetPosition - hit.position).normalized);
        }

        // сортировка массива по дистанции от игрока
        private int ColliderArraySortComparer(Collider a, Collider b)
        {
            if (a == null && b != null)
            {
                return 1;
            }
            else if (a != null && b == null)
            {
                return -1;
            }
            else if (a == null && b == null)
            {
                return 0;
            }
            else
            {
                var position = agent.transform.position;
                return (position - a.transform.position).sqrMagnitude.CompareTo((position - b.transform.position)
                    .sqrMagnitude);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (data.FoundInvestigatePoint == null && data.FoundCoverPos == null)
            {
                return;
            }

            foreach (Points investPos in data.FoundInvestigatePoint)
            {
                if (investPos.PointPosition == Vector3.zero) //Null Check
                {
                    return;
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(investPos.PointPosition, 1);
            }

            // foreach (Points coverPos in data.FoundCoverPos)
            // {
            //     if (coverPos.PointPosition == Vector3.zero) //Null Check
            //     {
            //         return;
            //     }
            //
            //     Gizmos.color = Color.green;
            //     Gizmos.DrawSphere(coverPos.PointPosition, 1);
            //     Gizmos.DrawRay(coverPos.ShootPos, (data.TargetAimPos - coverPos.ShootPos));
            // }
        }
    }
}