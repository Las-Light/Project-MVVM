using UnityEngine;
using UnityEngine.AI;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem
{
    public class CoverPointSystem: SearchManager
    {
        [Space(10)]
        [Header("Sensitivity hiding spot")]
        [Range(-1, 1)]
        [Tooltip("Lower is a better hiding spot")]
        [SerializeField]
        private float maxSens = -0.2f;
        [Range(-1, 1)]
        [Tooltip("Higher is a better hiding spot")]
        [SerializeField] private float minSens = -1f;
        
        [Header("Distance and height cover props")]
        [Range(0, 10)] [SerializeField] private float minCoverPlaceDistance = 4f;
        [Range(1, 25)] [SerializeField] private float maxCoverPlaceDistance = 15f;
        [Range(0, 5f)] [SerializeField] private float minCoverObstacleHeight = 0.39f;
        [Range(0, 5f)] [SerializeField] private float maxCoverObstacleHeight = 1.8f;
        [Header("Radius search area")]
        [Range(1, 25)] [SerializeField] private float searchRadiusAreas = 15f;
        public void StartSearchCovers()
        {
            SettingsCheckArea(minSens, maxSens, minCoverPlaceDistance, maxCoverPlaceDistance, minCoverObstacleHeight,
                maxCoverObstacleHeight, searchRadiusAreas);
            CheckAreaAndFindPoints(data.TargetLastKnownPosition);
        }

        public void StopSearchCovers()
        {
            patrolManager.StopCoverAction();
        }

        // проверка следующего укрытия на занятость и если находит подходящее укрытие то устанавливает его в CurrentCover
        // и возвращает true, если не одна из точек не подходит возвращает false
        public bool CheckNextCoverPoint()
        {
            int index = patrolManager.CurrentPatrolPoint.Index;
            if (patrolManager.PatrolPointList.Count > 1)
            {
                for (int i = 0; i < patrolManager.PatrolPointList.Count; i++)
                {
                    if (combatController.CheckNextCoverPointForOccupied(gameObject, patrolManager.PatrolPointList,
                            patrolManager.PatrolPointList[index]))
                    {
                        if (!patrolManager.PatrolPointList[patrolManager.PatrolPointList[index].NextIndex]
                                .HasBeenPassed || worldData.IsIgnoreCoverPassed)
                        {
                            data.CurrentCover =
                                patrolManager.PatrolPointList[patrolManager.PatrolPointList[index].NextIndex];

                            return true;
                        }

                        worldData.IsCoverPointPassed = true;
                    }

                    index = patrolManager.PatrolPointList[index].NextIndex;
                }

                return false;
            }

            return false;
        }

        // проверяет текущее укрытие на соответсвие положению по отношению к цели, если соответствует, то возвращает true 
        public bool CheckCoverByDot()
        {
            if (data.CurrentCover != null)
            {
                NavMesh.FindClosestEdge(data.CurrentCover.PointPosition, out NavMeshHit hit, agent.areaMask);
                if (DotHitPoint(hit, data.CurrentEnemy.transform.position) > maxSens ||
                    (data.CurrentCover.PointPosition - transform.position).sqrMagnitude > 1)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        // проверяет передаваемое укрытие на соответсвие положению по отношению к цели, если соответствует, то возвращает true 
        private bool CheckCoverByDot(Points coverPoint)
        {
            NavMesh.FindClosestEdge(coverPoint.PointPosition, out NavMeshHit hit, agent.areaMask);
            if (DotHitPoint(hit, data.CurrentEnemy.transform.position) > maxSens)
            {
                return true;
            }

            return false;
        }
    }
}