using System.Collections;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Agent;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.SensorySystem
{
    public class UnitSight : MonoBehaviour
    {
        [SerializeField] [Range(0, 359)] private float _focalViewAngle; //FOV
        [SerializeField] [Range(0, 359)] private float _peripheralViewAngle; //угол периферийного зрения
        [SerializeField] [Range(0, 30)] private float _viewRadius; //The range I can see
        [SerializeField] [Range(0, 30)] private float _peripheralViewDistance; //The range I can see peripheral
        [SerializeField] private Transform _viewPoint;

        public LayerMask ObstacleMask; //What are obstacles
        public LayerMask TargetMask; //What sh
        public LayerMask PlayerMask; //What sh

        public TargetType[] Targets;
        public TargetType Ally;

        private bool _shouldBeInvestigating = false; //Should I be investigating right now

        private AIAgent _aiAgent;
        private PatrolManager _patrolManager;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private CharacterView _characterView;

        [SerializeField] public Vector3 _offset = new Vector3(0f, 1.5f, 0f);

        public float FocalViewAngle => _focalViewAngle;

        public float ViewRadius => _viewRadius;

        public Transform ViewPoint => _viewPoint;

        //public List<GameObject> VisibleTargets => _visibleTargets;

        //TODO эвент на удаление из списка видимых целей Action OnEnemyKilled когда игрок или собака умрут

        private void Awake()
        {
            _aiAgent = GetComponent<HumanAgent>();
            _patrolManager = GetComponent<PatrolManager>();
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _characterView = GetComponent<CharacterView>();
            _viewPoint = _characterView.pointToCheckClip;
        }

        private void Start()
        {
            StartCoroutine("FindTargets", 0.5f);
        }

        private IEnumerator FindTargets(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }

        private void FindVisibleTargets()
        {
            _data.VisibleTargets.Clear(); //Clear the targets list

            Collider[] targetsInViewRadius = new Collider[50];

            int size = Physics.OverlapSphereNonAlloc(transform.position, _viewRadius, targetsInViewRadius, TargetMask);

            for (int i = 0; i < size; i++) //Loop through this list
            {
                GameObject target = targetsInViewRadius[i].gameObject; //Set the target to the current iteration
                target.TryGetComponent(out EnemyData targetEnemyData);
                target.TryGetComponent(out EnemyWorldData targetWorldData);

                if (target.transform.childCount > 0)
                {
                    Vector3 dirToTarget =
                        ((target.transform.GetChild(0).position) -
                         _viewPoint.position)
                        .normalized; //Get the direction from me to the target

                    float distanceToTarget =
                        Vector3.Distance(_viewPoint.position,
                            target.transform.GetChild(0)
                                .position); //Calculate distance from me to target


                    if (Vector3.Angle(_viewPoint.forward, dirToTarget) <
                        _focalViewAngle * 0.5f) //Check if the target is in my fov
                    {
                        if (!Physics.Raycast(_viewPoint.position, dirToTarget, distanceToTarget,
                                ObstacleMask)) //Check if its in my range
                        {
                            CheckEnemyOfSight(target);
                            CheckAllyKnowsAboutEnemy(target, targetWorldData, targetEnemyData);
                        }
                    }

                    if (Vector3.Angle(_viewPoint.forward, dirToTarget) < _peripheralViewAngle * 0.5f &&
                        distanceToTarget < _peripheralViewDistance)
                    {
                        if (!Physics.Raycast(_viewPoint.position, dirToTarget, distanceToTarget,
                                ObstacleMask))
                        {
                            CheckEnemyOfSight(target);
                            CheckAllyKnowsAboutEnemy(target, targetWorldData, targetEnemyData);
                        }
                    }
                }
            }

            ClosestTarget();
            CheckEnemy();
            TargetLost();
        }
        
        // ищет среди видимых целей, цель близжайшую к игроку
        private GameObject ClosestTarget()
        {
            if (_data.VisibleTargets.Count > 0)
            {
                float minDistance = Mathf.Infinity;
                GameObject closestTarget = null;

                for (int i = 0; i < _data.VisibleTargets.Count; i++)
                {
                    // прицеливание на ближайшего врага по дистанции
                    float targetDistance =
                        Vector3.Distance(transform.position, _data.VisibleTargets[i].transform.position);
                    if (targetDistance < minDistance)
                    {
                        minDistance = targetDistance;
                        closestTarget = _data.VisibleTargets[i];
                    }
                }

                if (closestTarget != _data.CurrentEnemy)
                {
                    if (_worldData.IsMove)
                    {
                        Debug.Log("Interrupt-UnitSight1");
                        _aiAgent.Interrupt = true;
                    }

                    _data.CurrentEnemy = closestTarget;
                }

                if (closestTarget)
                {
                    _data.TargetLastKnownPosition = closestTarget.transform.position;
                    _data.TargetLastKnownDirection = closestTarget.transform.forward;
                    _data.TargetLastKnownDetectionTime = Time.time;
                    if (!_worldData.IsNeedRotate)
                    {
                        if (_data.WeaponType != WeaponType.Melee)
                        {
                            _worldData.IsNeedAttack = true;
                        }

                        if (_worldData.IsNeedClosely)
                        {
                            if (_worldData.IsTargetClosely) _worldData.IsNeedAttack = true;
                            else _worldData.IsNeedAttack = false;
                        }
                    }
                }

                SetEnemyType(closestTarget);
                if (!_worldData.IsCouldSeeEnemy)
                {
                    _worldData.IsCouldSeeEnemy = true;
                    _worldData.IsNeedVoice = true;
                    _worldData.ResetInvestigationState();
                    _worldData.ResetCheckAreaState();
                    _worldData.IsHearingSound = false;
                    _worldData.IsFoundAboutTarget = false;
                    GlobalEventManager.SendSeenTarget(gameObject, closestTarget);
                    if (_worldData.IsMove)
                    {
                        Debug.Log("Interrupt-UnitSight1");
                        _aiAgent.Interrupt = true;
                    }
                }

                return closestTarget;
            }

            return null;
        }

        private void CheckEnemy()
        {
            if (_data.VisibleTargets.Count == 0)
            {
                _worldData.IsSeeEnemy = false;
                _worldData.IsNeedAttack = false;
            }
        }

        private void CheckEnemyOfSight(GameObject target)
        {
            for (int i = 0; i < Targets.Length; i++)
            {
                if (target.CompareTag(Targets[i].ToString())) //If it is the player
                {
                    _worldData.IsSeeEnemy = true;
                    _worldData.IsTargetLost = false;

                    _data.VisibleTargets.Add(target); //Add that target to the list
                }
            }
        }

        private void CheckAllyKnowsAboutEnemy(GameObject target, EnemyWorldData targetEnemyWorldData,
            EnemyData targetEnemyData)
        {
            if (target.CompareTag(Ally.ToString()) && target != gameObject)
            {
                if (!_worldData.IsCouldSeeEnemy && targetEnemyWorldData && targetEnemyWorldData.IsSeeEnemy &&
                    (!_worldData.IsNeedFlank || _worldData.BaseRole != BaseRole.CheckFlank) && !_worldData.IsFoundAboutTarget)
                {
                    _worldData.IsFoundAboutTarget = true;
                    _data.TargetLastKnownPosition =
                        targetEnemyData.TargetLastKnownPosition;
                    _data.TargetLastKnownDirection = targetEnemyData.transform.forward;
                    _data.TargetLastKnownDetectionTime = Time.time;
                    GlobalEventManager.SendFoundAboutTarget(gameObject);
                    if (_worldData.IsMove)
                    {
                        _aiAgent.Interrupt = true; //Interrupt current action
                    }
                }
            }
        }

        private void TargetLost()
        {
            if (!_worldData.IsNeedRotate && _data.VisibleTargets.Count == 0 &&
                _worldData.IsCouldSeeEnemy && !_worldData.IgnoreTarget &&
                !_worldData.IsNeedSit) //If guard has lost sight of the player
            {
                _worldData.IsSeeEnemy = false;
                _worldData.IsTargetLost = true;
                _worldData.IsCouldSeeEnemy = false;
                if (_worldData.IsMove)
                {
                    Debug.Log("Interrupt-LostTarget " + gameObject.name);
                    _aiAgent.Interrupt = true;
                }

                GlobalEventManager.SendLostTarget(gameObject, _data.CurrentEnemy);
            }
        }

        // определяет какой тип врага он увидел
        private void SetEnemyType(GameObject target)
        {
            if (target.CompareTag(TargetType.Dog.ToString()))
            {
                _data.EnemyType = TargetType.Dog;
            }

            else if (target.CompareTag(TargetType.Player.ToString()))
            {
                _data.EnemyType = TargetType.Player;
            }

            else if (target.CompareTag(TargetType.Infected.ToString()))
            {
                _data.EnemyType = TargetType.Infected;
            }
        }

        public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }

            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0,
                Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
    }
}