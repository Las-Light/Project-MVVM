using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Agent;
using UnityEngine;
using UnityEngine.AI;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem
{
    public class PatrolManager : MonoBehaviour
    {
        public List<Points> PatrolPointList; //The list of patrol points

        private List<Points>
            _originalPatrolPoints; //To store the Patrol points in when the guard goes into investigation mode

        private Vector3 _patrolCenter; //The center point to localise the patrol around

        [SerializeField] private int amountOfPatrolPoints; //The amount of patrol points

        [SerializeField] private float patrolRange; //The range the guard can patrol at

        [SerializeField] private LayerMask obstacleMask;

        public GameObject PatrolPointsTarget;
        public Points CurrentPatrolPoint;

        private NavMeshAgent _navMeshAgent; //Reference to the Navmesh agent
        private AIAgent _aiAgent;
        private EnemyWorldData _worldData;
        private EnemyData _data;

        private bool _movingMode;
        private bool _investigationMode;
        private bool _coverMode;
        private bool _checkAreaMode;

        private int _doorMask;

        private void Awake()
        {
            PatrolPointList = new List<Points>(); //Declare patrol list
            Points placeholders = new Points(); //Declare patrol list

            //Initialise to prevent other scripts accessing this one from crashing before points have been made.
            for (int i = 0; i < amountOfPatrolPoints; i++)
            {
                PatrolPointList.Add(placeholders);
            }

            _navMeshAgent = GetComponent<NavMeshAgent>();
            _aiAgent = GetComponent<HumanAgent>();
            _worldData = GetComponent<EnemyWorldData>();
            _data = GetComponent<EnemyData>();
            _doorMask = 1 << NavMesh.GetAreaFromName("Door");
        }

        // Start is called before the first frame update
        private void Start()
        {
            ReCalculatePatrol(); //Calculate the first route

            PatrolPointsTarget = new GameObject("GuardTarget " + gameObject.name);
            PatrolPointsTarget.transform.position = transform.position;
            CurrentPatrolPoint = GetSinglePatrolPoint(0);
            _navMeshAgent.SetAreaCost(14, 1);
        }

        public GameObject GetCurrentPatrolPoint()
        {
            if (CurrentPatrolPoint.PointPosition == Vector3.zero)
            {
                CurrentPatrolPoint.PointPosition = transform.position;
            }

            PatrolPointsTarget.transform.position = CurrentPatrolPoint.PointPosition;
            CurrentPatrolPoint.HasBeenPassed = true;

            return PatrolPointsTarget;
        }

        /// <summary>
        /// Returns the Patrol point with the given index
        /// </summary>
        public Points GetSinglePatrolPoint(int index)
        {
            return PatrolPointList[index];
        }

        public void NextPatrolPoint()
        {
            CurrentPatrolPoint = GetSinglePatrolPoint(CurrentPatrolPoint.NextIndex);
        }

        public void NextPatrolPoint(int index)
        {
            CurrentPatrolPoint.HasBeenPassed = true;
            CurrentPatrolPoint = GetSinglePatrolPoint(index);
        }

        public void MoveTarget(Vector3 pos)
        {
            PatrolPointsTarget.transform.position = pos;
        }

        public GameObject GetCheckTarget()
        {
            return PatrolPointsTarget;
        }

        public void SetPatrolCenter(Vector3 position)
        {
            _patrolCenter = position;
        }

        public void StopInvestigationAction()
        {
            if (_investigationMode && !_movingMode && !_coverMode)
            {
                PatrolPointList.Clear();
                _data.FoundInvestigatePoint.Clear();
                PatrolPointList = new List<Points>(_originalPatrolPoints);
                ResetCurrentPatrolPoint();
            }

            _investigationMode = false;
            if (!_worldData.IsSeeEnemy)
            {
                _data.TargetLastKnownPosition = Vector3.zero;
                _data.TargetLastKnownDirection = Vector3.zero;
                _data.TargetLastKnownDetectionTime = 0f;
                _data.HeardSoundPosition = Vector3.zero;
                _data.SoundDetectionTime = 0f;
            }

            Debug.Log("StopInvest " + gameObject.name);
        }

        public void StopCheckAreaAction()
        {
            if (_checkAreaMode && !_investigationMode && !_coverMode && !_movingMode)
            {
                PatrolPointList.Clear();
                PatrolPointList = new List<Points>(_originalPatrolPoints);
                ResetCurrentPatrolPoint();
            }

            _checkAreaMode = false;
            Debug.Log("StopCheckArea " + gameObject.name);
        }

        public void StopCoverAction()
        {
            if (_coverMode && !_investigationMode && !_movingMode)
            {
                PatrolPointList.Clear();
                _data.FoundCoverPos.Clear();
                PatrolPointList = new List<Points>(_originalPatrolPoints);
                ResetCurrentPatrolPoint();
            }

            _data.CurrentCover = null;
            _coverMode = false;
            Debug.Log("StopCover " + gameObject.name);
        }

        public void StopMoveAction()
        {
            if (_movingMode && !_investigationMode && !_coverMode)
            {
                PatrolPointList.Clear();
                PatrolPointList = new List<Points>(_originalPatrolPoints);
                ResetCurrentPatrolPoint();
            }

            _movingMode = false;
            Debug.Log("StopMove " + gameObject.name);
        }

        public void ResetAllPatrolMode()
        {
            _movingMode = false;
            _coverMode = false;
            _investigationMode = false;
            _checkAreaMode = false;
        }

        public bool CreateRouteUseSpecifiedPoints(int amountOfPoints, List<Points> foundPoints)
        {
            if (!_movingMode && !_investigationMode && !_coverMode && !_checkAreaMode)
            {
                _originalPatrolPoints = new List<Points>(PatrolPointList); //Save the old patrol points
            }

            ResetAllPatrolMode();

            if (_worldData.IsNeedInvestigation) _investigationMode = true;
            if (_worldData.IsNeedCheckArea) _checkAreaMode = true;
            if (_worldData.IsNeedCover) _coverMode = true;

            PatrolPointList.Clear(); //Clear the patrol list to start a new one
            GenerateSpecifiedPositions(amountOfPoints, foundPoints); //Generate the positions for this new point
            if (ChooseRouteInvestigate()) //Choose a "coherent" route around the points
            {
                ResetCurrentPatrolPoint(); //Reset other variables
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tells the unit to investigate an area
        /// It will save the old patrol points and generate a smaller patrol path around the centralPos
        /// </summary>
        public bool CreateRouteUseRandomPoints(Vector3 centralPos, int amountOfPoints, float rangeToInvestigate)
        {
            if (!_movingMode && !_investigationMode && !_coverMode && !_checkAreaMode)
            {
                _originalPatrolPoints = new List<Points>(PatrolPointList); //Save the old patrol points
            }

            ResetAllPatrolMode();

            _patrolCenter = centralPos; //Set the center
            PatrolPointList.Clear(); //Clear the patrol list to start a new one
            if (GenerateRandomPositions(amountOfPoints, rangeToInvestigate) &&
                ChooseRoute()) //Choose a "coherent" route around the points
            {
                if (_worldData.IsNeedInvestigation) _investigationMode = true;
                if (_worldData.IsNeedCheckArea) _checkAreaMode = true;
                if (_worldData.IsNeedCover) _coverMode = true;
                if (_worldData.IsNeedShootPos) _movingMode = true;
                ResetCurrentPatrolPoint(); //Reset other variables
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tells the unit to shoot position
        /// It will save the old patrol points and generate a smaller patrol path around the centralPos
        /// </summary>
        public bool CreateRandomShootPoint(Vector3 centralPos, float rangeToInvestigate)
        {
            if (!_movingMode && !_investigationMode && !_coverMode && !_checkAreaMode)
            {
                _originalPatrolPoints = new List<Points>(PatrolPointList); //Save the old patrol points
            }

            ResetAllPatrolMode();

            _patrolCenter = centralPos; //Set the center
            PatrolPointList.Clear(); //Clear the patrol list to start a new one
            if (GenerateRandomPosForShoot(rangeToInvestigate))
            {
                if (_worldData.IsNeedInvestigation) _investigationMode = true;
                if (_worldData.IsNeedCheckArea) _checkAreaMode = true;
                if (_worldData.IsNeedCover) _coverMode = true;
                if (_worldData.IsNeedShootPos) _movingMode = true;
                ResetCurrentPatrolPoint(); //Reset other variables
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tells the unit to investigate an area
        /// It will save the old patrol points and generate a smaller patrol path around the centralPos
        /// </summary>
        public void CreateSelfPoint(Vector3 centralPos)
        {
            if (!_movingMode && !_investigationMode && !_coverMode && !_checkAreaMode)
            {
                _originalPatrolPoints = new List<Points>(PatrolPointList); //Save the old patrol points
            }

            ResetAllPatrolMode();

            if (_worldData.IsNeedInvestigation) _investigationMode = true;
            if (_worldData.IsNeedCheckArea) _checkAreaMode = true;
            if (_worldData.IsNeedCover) _coverMode = true;
            if (_worldData.IsNeedShootPos) _movingMode = true;

            PatrolPointList.Clear(); //Clear the patrol list to start a new one
            GenerateSelfPositions(centralPos); //Generate the positions for this new point
            ResetCurrentPatrolPoint(); //Reset other variables
        }

        /// <summary>
        /// Tells the unit move to side
        /// It will save the old patrol points and generate a smaller patrol path around the move point
        /// </summary>
        public bool CreateRouteForMoveToSide(Transform transformUnit)
        {
            if (!_movingMode && !_investigationMode && !_coverMode && !_checkAreaMode)
            {
                _originalPatrolPoints = new List<Points>(PatrolPointList); //Save the old patrol points
            }

            ResetAllPatrolMode();

            PatrolPointList.Clear(); //Clear the patrol list to start a new one
            if (GenerateSidePositions(transformUnit))
            {
                if (_worldData.IsNeedFireLine) _movingMode = true;
                if (ChooseRoute()) //Choose a "coherent" route around the points
                    ResetCurrentPatrolPoint(); //Reset other variables

                return true;
            }

            return false;
        }

        public bool GetFlankPosition(Vector3 targetPos)
        {
            float distanceToTarget = (targetPos - transform.position).magnitude;
            Vector3 raycastTarget = new Vector3(targetPos.x, 1.634f, targetPos.z);
            Vector3 raycastPoint;
            // ищет точки слева от игрока которые не закрыты препятствием
            if (CreateRouteUseRandomPoints(raycastPoint = GetLeftFlankPoint(
                    targetPos, transform.position, distanceToTarget), 1, distanceToTarget / 3) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            // если слева не нашлись точки то ищет точки справа
            if (CreateRouteUseRandomPoints(raycastPoint = GetRightFlankPoint(
                    targetPos, transform.position, distanceToTarget), 1, distanceToTarget / 3) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            // если слева не нашлись точки то ищет точки ближе к цели
            if (CreateRouteUseRandomPoints(raycastPoint = GetLeftFlankPoint(
                    targetPos, transform.position, distanceToTarget / 2), 1, distanceToTarget / 3) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            // если слева не нашлись точки то ищет точки справа
            if (CreateRouteUseRandomPoints(raycastPoint = GetRightFlankPoint(
                    targetPos, transform.position, distanceToTarget / 2), 1, distanceToTarget / 3) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            if (CreateRouteUseRandomPoints(raycastPoint = GetLeftFlankPoint(
                    targetPos, transform.position, distanceToTarget / 3), 1, distanceToTarget / 4) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            if (CreateRouteUseRandomPoints(raycastPoint = GetRightFlankPoint(
                    targetPos, transform.position, distanceToTarget / 3), 1, distanceToTarget / 4) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            if (CreateRouteUseRandomPoints(raycastPoint = GetLeftFlankPoint(
                    targetPos, transform.position, distanceToTarget / 4), 1, distanceToTarget / 4) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            if (CreateRouteUseRandomPoints(raycastPoint = GetRightFlankPoint(
                    targetPos, transform.position, distanceToTarget / 4), 1, distanceToTarget / 4) &&
                !Physics.Raycast(raycastTarget,
                    raycastPoint = new Vector3(raycastPoint.x, 1.634f, raycastPoint.z) - raycastTarget,
                    raycastPoint.magnitude, obstacleMask))
            {
                return true;
            }

            return false;
        }

        public Vector3 GetShootFrontPoint(Vector3 targetPos, Vector3 selfPos, float distToTarget)
        {
            Vector3 dirToTarget = (targetPos - selfPos).normalized;
            float distToPoint = 0;
            if (distToTarget >= 7 && distToTarget <= 12) distToPoint = distToTarget / 1.5f;
            if (distToTarget > 12) distToPoint = distToTarget / 2;
            if (distToTarget < 7) distToPoint = distToTarget / 3;
            Vector3 centralPoint = selfPos + dirToTarget * distToPoint;
            return centralPoint;
        }
        
        public Vector3 GetShootBackPoint(Vector3 targetPos, Vector3 selfPos, float distToTarget)
        {
            Vector3 dirToTarget = (targetPos - selfPos).normalized;
            float distToPoint = 0;
            if (distToTarget >= 7 && distToTarget <= 12) distToPoint = distToTarget / 1.5f;
            if (distToTarget > 12) distToPoint = distToTarget / 2;
            if (distToTarget < 7) distToPoint = distToTarget / 3;
            Vector3 centralPoint = selfPos - dirToTarget * distToPoint;
            return centralPoint;
        }

        private Vector3 GetLeftFlankPoint(Vector3 targetPos, Vector3 selfPos, float distToTarget)
        {
            Vector3 dirToTarget = (targetPos - selfPos).normalized;
            Vector3 flankDir = new Vector3(dirToTarget.z, 0, -dirToTarget.x);
            float distToPoint = 0;
            if (distToTarget >= 7 && distToTarget <= 12) distToPoint = distToTarget / 1.5f;
            if (distToTarget > 12) distToPoint = distToTarget / 2;
            if (distToTarget < 7) distToPoint = distToTarget;
            Vector3 centralPoint = targetPos + flankDir * distToPoint;
            return centralPoint;
        }

        private Vector3 GetRightFlankPoint(Vector3 targetPos, Vector3 selfPos, float distToTarget)
        {
            Vector3 dirToTarget = (targetPos - selfPos).normalized;
            Vector3 flankPos = new Vector3(-dirToTarget.z, 0, dirToTarget.x);
            float distToPoint = 0;
            if (distToTarget >= 7 && distToTarget <= 12) distToPoint = distToTarget / 1.5f;
            if (distToTarget > 12) distToPoint = distToTarget / 2;
            if (distToTarget < 7) distToPoint = distToTarget;
            Vector3 centralPoint = targetPos + flankPos * distToPoint;
            return centralPoint;
        }

        private void ResetCurrentPatrolPoint()
        {
            CurrentPatrolPoint = GetSinglePatrolPoint(0);
        }

        private void ReCalculatePatrol()
        {
            _movingMode = false;
            _coverMode = false;
            _investigationMode = false;
            _checkAreaMode = false;
            _patrolCenter = transform.position;
            PatrolPointList.Clear();
            GenerateRandomPositions(amountOfPatrolPoints, patrolRange);
            ChooseRoute();
            if (_worldData.IsMove)
            {
                _aiAgent.Interrupt = true;
            }
        }

        /// <summary>
        /// Function to find the shortest route to each pathpoint
        /// </summary>
        private bool ChooseRoute()
        {
            if (PatrolPointList.Count > 0)
            {
                FindNearestUnusedNeighbor(1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Function to find the shortest route to each pathpoint
        /// </summary>
        private bool ChooseRouteInvestigate()
        {
            if (PatrolPointList.Count > 0)
            {
                FindNearestSpecifiedPoint();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates the self positions and checks if they are validly placed on the navmesh
        /// </summary>
        /// <param name="selfPos">Position unit in now</param>
        private void GenerateSelfPositions(Vector3 selfPos)
        {
            Points selfPoint = new Points(); //Temporary placeholder to add later
            selfPoint.PointPosition = selfPos;
            selfPoint.Index = 0;
            PatrolPointList.Add(selfPoint);
        }

        /// <summary>
        /// Generates the random positions and checks if they are validly placed on the navmesh
        /// </summary>
        /// <param name="amountOfPoints">The amount of patrol points</param>
        /// <param name="range">The range the points can be generated away from the center point</param>
        private bool GenerateRandomPositions(int amountOfPoints, float range)
        {
            bool foundPoint = false;
            for (int i = 0; i < amountOfPoints; i++) //Loop through the points
            {
                Points patrolPlaceholder = new Points(); //Temporary placeholder to add later
                NavMeshPath path = new NavMeshPath(); //Create new nav path
                bool canReachTarget = false; //Can the point actually be reached
                float iteration = 20;
                while (iteration >= 0 && canReachTarget == false) //Loop until point is valid
                {
                    iteration -= 1;
                    patrolPlaceholder.SetRandomPos(_patrolCenter, range); //Get random position

                    _navMeshAgent.CalculatePath(patrolPlaceholder.PointPosition, path);
                    if (path.status == NavMeshPathStatus.PathPartial ||
                        path.status == NavMeshPathStatus.PathInvalid || CheckPositionAsOccupied(patrolPlaceholder.PointPosition)) //If path is not valid
                    {
                        Debug.LogError("Bad Point");
                    }
                    else
                    {
                        //проверку raycast на отсутсвие стен между точками
                        if (!NavMesh.Raycast(_patrolCenter, patrolPlaceholder.PointPosition, out _,
                                NavMesh.AllAreas))
                        {
                            canReachTarget = true;
                        }
                    }
                }

                patrolPlaceholder.Index = i; //Set index of point
                PatrolPointList.Add(patrolPlaceholder); //Add it to list
                if (canReachTarget)
                {
                    foundPoint = true;
                }
            }

            return foundPoint;
        }

        /// <summary>
        /// Generates the random positions and checks if they are validly placed on the navmesh
        /// </summary>
        /// <param name="range">The range the points can be generated away from the center point</param>
        private bool GenerateRandomPosForShoot(float range)
        {
            Points shootPos = new Points(); //Temporary placeholder to add later
            NavMeshPath path = new NavMeshPath(); //Create new nav path
            bool canReachTarget = false; //Can the point actually be reached
            float iteration = 20;
            while (iteration >= 0 && canReachTarget == false) //Loop until point is valid
            {
                iteration -= 1;
                shootPos.SetRandomPos(_patrolCenter, range); //Get random position

                _navMeshAgent.CalculatePath(shootPos.PointPosition, path);
                shootPos.ShootPos = new Vector3(shootPos.PointPosition.x, 1.634f, shootPos.PointPosition.z);
                if (path.status == NavMeshPathStatus.PathPartial ||
                    path.status == NavMeshPathStatus.PathInvalid ||
                    NavMesh.Raycast(_patrolCenter, shootPos.PointPosition, out NavMeshHit hit,
                        NavMesh.AllAreas) || CheckToFireLine(shootPos) || CheckPositionAsOccupied(shootPos.PointPosition)) //If path is not valid
                {
                    Debug.LogError("Bad Point");
                }
                else
                {
                    canReachTarget = true;
                }
            }

            shootPos.Index = 0; //Set index of point
            PatrolPointList.Add(shootPos); //Add it to list
            return canReachTarget;
        }

        /// <summary>
        /// Generates the side positions (left or right from unit) and checks if they are validly placed on the navmesh
        /// </summary>
        /// <param name="centerTransform">The Transform unit who needs to move side</param>
        private bool GenerateSidePositions(Transform centerTransform)
        {
            Points sidePoint = new Points(); //Temporary placeholder to add later
            NavMeshPath path = new NavMeshPath(); //Create new nav path
            float iteration = 20;
            bool canReachTarget = false; //Can the point actually be reached
            while (iteration >= 0 && canReachTarget == false) //Loop until point is valid
            {
                iteration -= 1;
                sidePoint.SetSidePos(centerTransform); //Get random position

                _navMeshAgent.CalculatePath(sidePoint.PointPosition, path);
                sidePoint.ShootPos = new Vector3(sidePoint.PointPosition.x, 1.634f, sidePoint.PointPosition.z);
                if ((path.status == NavMeshPathStatus.PathPartial ||
                     path.status == NavMeshPathStatus.PathInvalid) || CheckToFireLine(sidePoint) ||
                    CheckPositionAsOccupied(sidePoint.PointPosition)) //If path is not valid
                {
                    Debug.LogError("Bad Point");
                }
                else
                {
                    //проверку raycast на отсутсвие стен между точками
                    if (!NavMesh.Raycast(centerTransform.position, sidePoint.PointPosition, out NavMeshHit hit,
                            NavMesh.AllAreas))
                    {
                        canReachTarget = true;
                    }
                }
            }

            sidePoint.Index = 0; //Set index of point
            PatrolPointList.Add(sidePoint); //Add it to list
            return canReachTarget;
        }

        /// <summary>
        /// Generates the positions and checks from foundPoints if they are validly placed on the navmesh
        /// </summary>
        /// <param name="amountOfPoints">Count FoundInvestigatePoint</param>
        /// <param name="foundPoints">List of FoundInvestigatePoint</param>
        private void GenerateSpecifiedPositions(int amountOfPoints, List<Points> foundPoints)
        {
            for (int i = 0; i < amountOfPoints; i++) //Loop through the points
            {
                NavMeshPath path = new NavMeshPath(); //Create new nav path

                Points patrolPlaceholder = foundPoints[i];

                _navMeshAgent.CalculatePath(patrolPlaceholder.PointPosition, path);
                if (path.status == NavMeshPathStatus.PathPartial ||
                    path.status == NavMeshPathStatus.PathInvalid) //If path is not valid
                {
                    Debug.LogError("Bad Point");
                    return;
                }

                PatrolPointList.Add(patrolPlaceholder); //Add it to list
            }
        }

        private int FindNearestUnusedNeighbor(int index)
        {
            bool testBool = true;
            int nextIndex = 0;
            int infiniteLoopSafety = 0;
            while (testBool)
            {
                Points closestPoint = null; //Temp storage for closest Point
                float distanceToPoint = 0.0f;
                foreach (Points point in PatrolPointList)
                {
                    if (point.Index == nextIndex || point.HasBeenLinked)
                    {
                        continue;
                    }

                    if (closestPoint == null)
                    {
                        //Set first Point to closest, others will compare to this
                        closestPoint = point;
                        distanceToPoint = (point.PointPosition - PatrolPointList[nextIndex].PointPosition).magnitude;
                    }
                    else
                    {
                        //Checks if the current Point is closer than the last one
                        float dist = (point.PointPosition - PatrolPointList[nextIndex].PointPosition).magnitude;
                        if (dist < distanceToPoint)
                        {
                            //Use the closer Point instead
                            closestPoint = point;
                            distanceToPoint = dist;
                        }
                    }
                }

                if (closestPoint == null)
                {
                    closestPoint =
                        PatrolPointList
                            [0]; //For the last node, which wont beable to find a partner node, so it is set to the first one to create a loop
                }

                PatrolPointList[nextIndex].NextIndex = closestPoint.Index; //Link partner
                PatrolPointList[nextIndex].HasBeenLinked = true; //Linked
                nextIndex = closestPoint.Index; //For the next point in route

                if (RouteFinishCheck()) //If route has finished
                {
                    testBool = false; //Exit loop
                }

                //I put this in because I'm an idiot and kept crashing unity.
                infiniteLoopSafety++;
                if (infiniteLoopSafety > amountOfPatrolPoints + 10)
                {
                    testBool = false;
                    Debug.Log("InfiniteLoop");
                }
            }

            return 0;
        }

        private void FindNearestSpecifiedPoint()
        {
            bool testBool = true;
            int nextIndex = 0;
            int lastIndex = 0;
            int infiniteLoopSafety = 0;
            Points closestPoint = null; //Temp storage for closest Point
            float distanceToPoint = 0.0f;
            foreach (Points point in PatrolPointList)
            {
                var path = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, point.PointPosition, NavMesh.AllAreas, path);
                float pathLenght = 0;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    //pathLenght += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                    pathLenght += (path.corners[i] - path.corners[i + 1]).sqrMagnitude;
                }

                if (closestPoint == null)
                {
                    closestPoint = point;
                    distanceToPoint = pathLenght;
                }
                else
                {
                    if (pathLenght < distanceToPoint)
                    {
                        closestPoint = point;
                        distanceToPoint = pathLenght;
                    }
                }
            }

            if (closestPoint != null)
            {
                closestPoint.Index = nextIndex;
                closestPoint.HasBeenLinked = true;
                closestPoint.DistToPoint = distanceToPoint;
                PatrolPointList.Remove(closestPoint);
                PatrolPointList.Insert(nextIndex, closestPoint);
            }

            while (testBool)
            {
                closestPoint = null;
                distanceToPoint = 0f;
                foreach (Points point in PatrolPointList)
                {
                    if (point.HasBeenLinked)
                    {
                        continue;
                    }

                    var path = new NavMeshPath();
                    NavMesh.CalculatePath(PatrolPointList[nextIndex].PointPosition, point.PointPosition,
                        NavMesh.AllAreas, path);
                    float pathLenght = 0;
                    for (int i = 0; i < path.corners.Length - 1; i++)
                    {
                        //pathLenght += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                        pathLenght += (path.corners[i] - path.corners[i + 1]).sqrMagnitude;
                    }

                    if (closestPoint == null)
                    {
                        closestPoint = point;
                        distanceToPoint = pathLenght;
                    }
                    else
                    {
                        if (pathLenght < distanceToPoint)
                        {
                            closestPoint = point;
                            distanceToPoint = pathLenght;
                        }
                    }
                }

                if (closestPoint == null)
                {
                    PatrolPointList[^1].NextIndex = 0;
                }
                else
                {
                    closestPoint.Index = nextIndex + 1;
                    PatrolPointList[nextIndex].NextIndex = closestPoint.Index; //Link partner
                    closestPoint.HasBeenLinked = true; //Linked
                    nextIndex++; //For the next point in route
                    PatrolPointList.Remove(closestPoint);
                    PatrolPointList.Insert(nextIndex, closestPoint);
                }

                if (RouteFinishCheck()) //If route has finished
                {
                    testBool = false; //Exit loop
                }

                //I put this in because I'm an idiot and kept crashing unity.
                infiniteLoopSafety++;
                if (infiniteLoopSafety > amountOfPatrolPoints + 10)
                {
                    testBool = false;
                    Debug.Log("InfiniteLoop");
                }
            }
        }

        private bool RouteFinishCheck()
        {
            foreach (Points point in PatrolPointList) //say ppPoint out loud
            {
                if (point.HasBeenLinked == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckToFireLine(Points point)
        {
            return Physics.Raycast(point.ShootPos,
                (_data.TargetAimPos - point.ShootPos).normalized,
                (point.ShootPos - _data.TargetAimPos).magnitude, obstacleMask);
        }

        private bool CheckPositionAsOccupied(Vector3 pos)
        {
            var isOccupied = false;
            foreach (var occupiedPos in _data.OccupiedPosition)
                if ((occupiedPos.Value - pos).sqrMagnitude < 4)
                    isOccupied = true;

            return isOccupied;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position

            foreach (Points patrolPoint in PatrolPointList)
            {
                if (patrolPoint == null) //Null Check
                {
                    return;
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(patrolPoint.PointPosition, 1);
            }

            //Draws a red sphere at the center of the investigation point
            if (_movingMode || _investigationMode || _coverMode || _checkAreaMode)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_patrolCenter, 1);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(_data.TargetLastKnownPosition, 1);
            }
        }
    }
}