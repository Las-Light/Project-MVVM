using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Agent;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Sound;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using UnityEngine;
using UnityEngine.AI;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController
{
    public class EnemyMovementController : MonoBehaviour
    {
        [SerializeField] private float MoveSpeed;
        [SerializeField] private float SprintSpeed;
        [SerializeField] private float AimSpeed;
        [SerializeField] private float RotationSpeed = 20f;

        [Space(10)]
        [Header("Player Grounded")]
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField]
        private float Gravity = -15.0f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        [SerializeField]
        private float FallTimeout = 0.15f;

        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField]
        private bool Grounded = true;

        [Tooltip("Useful for rough ground")] [SerializeField]
        private float GroundedOffset = -0.14f;


        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField]
        private float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")] [SerializeField]
        private LayerMask GroundLayers;

        [Space(10)] [Header("Aim and Clip points")] [Tooltip("ClipPervention Point")] [SerializeField]
        private Transform pointToCheckClip;

        [Tooltip("Target Aim Point")] [SerializeField]
        private Transform TargetAimPoint;

        [Tooltip("Speed blend Aim animation")] [SerializeField]
        private float SpeedBlendAim = 10f;

        [Header("CharacterController properties")] [Tooltip("Height Character Controller in crouch")] [SerializeField]
        private float crouchHeight;

        [Tooltip("Center Character Controller in crouch")] [SerializeField]
        private Vector3 crouchCenter;

        [Tooltip("Height Character Controller default")] [SerializeField]
        private float defaultHeight;

        [Tooltip("Center Character Controller default")] [SerializeField]
        private Vector3 defaultCenter;

        [SerializeField] private LayerMask obstacleMask;

        private RigController _rigController;
        private NavMeshAgent _navMeshAgent;
        private AnimatorController _humanAnimator;
        private EnemyData _data;
        private EnemyWorldData _worldData;
        private ArsenalView _arsenalView;
        private CharacterController _characterController;
        private SoundController _soundController;
        private AimController _aimController;
        private AIAgent _aiAgent;
        private CharacterView _characterView;

        private Vector3 _offset = new Vector3(0f, 1.5f, 0f);
        private float _speedBlendX;
        private float _speedBlendY;
        private bool _isCheckWall;
        private float _speedBlend;
        private float _attackTimer;
        private float _attackPeriod;
        private bool _aimTimerSet;
        private float _turnRotationAngle;

        private void Start()
        {
            _characterView = GetComponent<CharacterView>();
            _rigController = GetComponent<RigController>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _humanAnimator = GetComponent<AnimatorController>();
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _arsenalView = _characterView.ArsenalView;
            _soundController = GetComponent<SoundController>();
            _characterController = GetComponent<CharacterController>();
            _aimController = GetComponent<AimController>();
            _aiAgent = GetComponent<AIAgent>();
            NavMesh.avoidancePredictionTime = 5;
            _attackPeriod = Random.Range(0.5f, 2f);
        }

        //этот метод для RootMotion, без него игрок не движется
        private void OnAnimatorMove()
        {
        }

        public void Move(GameObject target)
        {
            if (target != null)
            {
                if (!_worldData.IsFindFireLine && !_worldData.IsFindShootPos)
                {
                    _navMeshAgent.updateRotation = true;
                }

                _navMeshAgent.SetDestination(target.transform.position);
                SwitchSpeed();

                AnimatedMove(_navMeshAgent.velocity);

                Debug.Log("Move to" + gameObject.name);
            }
        }

        public void AnimatedMove(Vector3 desiredVector)
        {
            if (_worldData.IsAim)
            {
                if (!_arsenalView.ClipPrevention(_worldData.IsAim, ref _isCheckWall))
                {
                    _aimController.SetAimPointForward();
                }
                _aimController.Aim(_arsenalView, _isCheckWall);

                Vector3 relativeVector = transform.InverseTransformDirection(desiredVector.normalized);

                _speedBlendX = Mathf.Lerp(_speedBlendX, relativeVector.x, Time.deltaTime * SpeedBlendAim);
                _speedBlendY = Mathf.Lerp(_speedBlendY, relativeVector.z, Time.deltaTime * SpeedBlendAim);
                _humanAnimator.AimMove(_speedBlendX, _speedBlendY);
            }
            else
            {
                _aimController.RemoveAim(_arsenalView);
                _speedBlend = Mathf.Lerp(_speedBlend, desiredVector.magnitude, Time.deltaTime * SpeedBlendAim);
                _humanAnimator.Move(_speedBlend);
            }
        }

        // вызывается из действия AttackActions, сначала проверяется нет ли препятствия перед юнитом, если есть меняет позицию
        // потом проверяет не перекрывает ли ему линию стрельбы другой юнит, если перекрывает, то ищет новую позицию
        // в атаке действует 3 таймера: 1. attackTimer (таймер атаки) - время которое юнит ведет атаку по окончанию которого действие атаки считается завершенным
        // 2. AimTimer (таймер прицеливания) - время прицеливания по игроку, по окончанию таймера производится выстрел (если таймер стрельбы закончился)
        // учитывается при расчете урона (чем дольше прицеливание тем больше урон);
        // 3. shootTimer (таймер стрельбы) - таймер того как быстро может стрелять оружие, 
        // устанавливается в weaponData в зависимости от типа оружия (учитывается в WeaponController)
        public bool FirearmsAttackTarget(GameObject target)
        {
            if (!_arsenalView.ClipPrevention(_worldData.IsAim, ref _isCheckWall))
            {
                _worldData.IsAim = true;
                AnimatedMove(_navMeshAgent.velocity);
                if (!RotateToTarget(target.transform.position))
                {
                    return false;
                }

                _humanAnimator.Aim(true);
                SetAimTimer();

                _rigController.AimRifleRig(true);

                _aimController.SetAimPointPosition(_data.TargetAimPos, _arsenalView.CurrentWeapon);

                if (Physics.Raycast(_data.SelfAimPos,
                        (_data.TargetAimPos - _data.SelfAimPos).normalized, out RaycastHit hitInfo))
                {
                    if (!hitInfo.collider.gameObject.CompareTag("Human"))
                    {
                        _attackTimer += Time.deltaTime;
                        _data.AimTimer -= Time.deltaTime;

                        if (_data.AimTimer <= 0)
                        {
                            if (_arsenalView.Shoot())
                            {
                                _aimTimerSet = false;
                            }
                        }

                        if (_attackTimer >= _attackPeriod && _data.AimTimer <= 0)
                        {
                            _attackPeriod = Random.Range(1f, 3f);
                            _attackTimer = 0f;
                            _aimTimerSet = false;
                            return true;
                        }

                        return false;
                    }

                    _worldData.IsNeedFireLine = true;
                    _worldData.IsTakeShootPos = false;
                    _worldData.IsCanAttack = false;
                    return false;
                }
            }
            else
            {
                _worldData.IsNeedFireLine = true;
                _worldData.IsTakeShootPos = false;
                _worldData.IsCanAttack = false;
            }

            return false;
        }

        public bool Voice()
        {
            if (_worldData.IsSeeEnemy)
                _soundController.MakeSoundNotify(_data.SelfAimPos, _data.TargetLastKnownPosition, SoundType.Notify);
            else
                _soundController.MakeSoundNotify(_data.SelfAimPos, transform.position, SoundType.Check);
            return true;
        }

        public void Wait()
        {
            //RotateToTarget(_data.CurrentEnemy.transform.position);
            AnimatedMove(_navMeshAgent.velocity);
            if ((_data.TargetLastKnownDetectionTime - Time.time) > 30)
            {
                _worldData.IsNeedWait = false;
                _worldData.IsNeedStay = false;
            }
        }

        public void IdleState()
        {
            AnimatedMove(Vector3.zero);
        }

        public void IdleStateExit()
        {
            throw new System.NotImplementedException();
        }

        public void StopMoving()
        {
            //_navMeshAgent.isStopped = true;
            AnimatedMove(_navMeshAgent.velocity);
        }

        public void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            _humanAnimator.Grounded(Grounded);
        }

        public bool RotateToTarget(Vector3 targetPosition)
        {
            AnimatedMove(_navMeshAgent.velocity);
            _navMeshAgent.updateRotation = false;
            Vector3 lookDirection = (targetPosition - transform.position).normalized;
            lookDirection.y = 0f;
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationSpeed);
            float turnAngle = Quaternion.Angle(transform.rotation, rotation);
            _turnRotationAngle = Vector3.SignedAngle(lookDirection, transform.forward, transform.up);

            if (turnAngle <= 2)
            {
                _humanAnimator.Turn(0, 0);
                return true;
            }

            _humanAnimator.Turn(_turnRotationAngle, (int)_turnRotationAngle);
            return false;
        }

        public bool Sit()
        {
            StopMoving();
            if (!_worldData.IsSit)
            {
                _worldData.IsSit = true;
                _humanAnimator.Crouch(true);
                pointToCheckClip.localPosition = new Vector3(0.2f, 0.95f, 0);
                TargetAimPoint.localPosition = new Vector3(0, 0.9f, 0);
                _characterController.height = crouchHeight;
                _characterController.center = crouchCenter;
                _data.CoverTimer = Random.Range(0f, 3f);
                GlobalEventManager.SendUpdatePosition(gameObject);
                return false;
            }

            if (_worldData.IsSit)
            {
                if (_data.CoverTimer <= 0)
                {
                    _worldData.IsSit = false;
                    _humanAnimator.Crouch(false);
                    pointToCheckClip.localPosition = new Vector3(0.2f, 1.6f, 0);
                    TargetAimPoint.localPosition = new Vector3(0, 1.4f, 0);
                    _characterController.height = defaultHeight;
                    _characterController.center = defaultCenter;
                    return true;
                }
            }

            _data.CoverTimer -= Time.deltaTime;
            return false;
        }

        private void SetAimTimer()
        {
            if (!_aimTimerSet)
            {
                _aimTimerSet = true;
                if (_worldData.IsNeedShootNow)
                {
                    _data.AimTimer = 0.5f;
                }
                else
                {
                    _data.AimTimer = Random.Range(0f, 1f);
                }
            }
        }

        private void SwitchSpeed()
        {
            if (_worldData.IgnoreTarget || _worldData.IsNeedFlank || _worldData.IsFoundAboutTarget)
            {
                _worldData.IsAim = false;
                _navMeshAgent.speed = SprintSpeed;
            }

            if (_worldData.BaseRole == BaseRole.CheckFlank)
            {
                _worldData.IsAim = false;
                _navMeshAgent.speed = SprintSpeed;
            }

            if (_worldData.IsAim)
            {
                _navMeshAgent.speed = AimSpeed;
            }

            if (_worldData.IsNeedPatrol)
            {
                _worldData.IsAim = false;
                _navMeshAgent.speed = MoveSpeed;
            }

            if (_worldData.IsNeedMoveBack)
            {
                _navMeshAgent.speed = AimSpeed;
            }
        }
    }
}