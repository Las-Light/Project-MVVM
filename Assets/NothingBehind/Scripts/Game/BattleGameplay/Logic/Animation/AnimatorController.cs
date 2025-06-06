using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation
{
    public class AnimatorController : MonoBehaviour, IAnimationStateReader
    {
        [SerializeField] private Animator _rigAnimator;
        // animation IDs
        private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIDGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimIDFreeFall = Animator.StringToHash("FreeFall");
        private static readonly int AnimIDMoveX = Animator.StringToHash("MoveX");
        private static readonly int AnimIDMoveY = Animator.StringToHash("MoveY");
        private static readonly int AnimIDAim = Animator.StringToHash("Aim");
        private static readonly int AnimIDCrouch = Animator.StringToHash("Crouch");
        private static readonly int AnimIDRifle = Animator.StringToHash("Rifle");
        private static readonly int AnimIDPistol = Animator.StringToHash("Pistol");
        private static readonly int AnimIDNotWeapon = Animator.StringToHash("NotWeapon");
        private static readonly int AnimIDTurnAngleFloat = Animator.StringToHash("TurnAngle_f");
        private static readonly int AnimIDTurnAngleInt = Animator.StringToHash("TurnAngle_int");
        private static readonly int AnimIDHit = Animator.StringToHash("Hit");
        private static readonly int AnimIDHitInt = Animator.StringToHash("Hit_int");
        private static readonly int AnimIDReload = Animator.StringToHash("Reload");
        private static readonly int AnimIDMeleeAttack = Animator.StringToHash("MeleeAttack");

        // animations state hash
        private readonly int _stateHashPutRifle = Animator.StringToHash("RiflePut");
        private readonly int _stateHashGetRifle = Animator.StringToHash("RifleGet");
        private readonly int _stateHashPutPistol = Animator.StringToHash("PistolPut");
        private readonly int _stateHashGetPistol = Animator.StringToHash("PistolGet");
        private readonly int _stateHashRifleRecoil = Animator.StringToHash("RifleShootRecoil");
        private readonly int _stateHashPistolRecoil = Animator.StringToHash("PistolShootRecoil");
        private readonly int _stateHashRifleAim = Animator.StringToHash("RifleAim");
        private readonly int _stateHashPistolAim = Animator.StringToHash("PistolAim");
        private readonly int _stateHashCrouchRifleNotAim = Animator.StringToHash("RifleCrouchNotAim");
        private readonly int _stateHashCrouchRifleAim = Animator.StringToHash("RifleCrouchAim");
        private readonly int _stateHashCrouchPistolNotAim = Animator.StringToHash("PistolCrouchNotAim");
        private readonly int _stateHashCrouchPistolAim = Animator.StringToHash("PistolCrouchAim");
        private readonly int _stateHashCrouchNotWeapon = Animator.StringToHash("NotWeaponCrouch");
        private readonly int _stateHashWalkNotWeapon = Animator.StringToHash("NotWeapon");
        private readonly int _stateHashWalkRifle = Animator.StringToHash("RifleNotAim");
        private readonly int _stateHashWalkPistol = Animator.StringToHash("PistolNotAim");
        private readonly int _stateHashHitStay1 = Animator.StringToHash("HitStay1");
        private readonly int _stateHashHitStay2 = Animator.StringToHash("HitStay2");
        private readonly int _stateHashHitStay3 = Animator.StringToHash("HitStay3");
        private readonly int _stateHashHitCrouch = Animator.StringToHash("HitCrouch");
        private readonly int _stateHashStartHit = Animator.StringToHash("StartHit");
        private readonly int _stateHashReload = Animator.StringToHash("Reload");
        private readonly int _stateHashReloadRig = Animator.StringToHash("ReloadRig");

        private Animator _animator;
        public AnimatorState State { get; private set; }

        public event Action<AnimatorState> StateEntered;
        public event Action<AnimatorState> StateExited;

        public void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Move(float speed)
        {
            _animator.SetFloat(AnimIDSpeed, speed);
        }

        public void AimMove(float moveX, float moveY)
        {
            _animator.SetFloat(AnimIDMoveX, moveX);
            _animator.SetFloat(AnimIDMoveY, moveY);
        }

        public void Aim(bool isAim)
        {
            _animator.SetBool(AnimIDAim, isAim);
        }

        public void Crouch(bool isCrouch)
        {
            _animator.SetBool(AnimIDCrouch, isCrouch);
        }

        public void FreeFall(bool isFall)
        {
            _animator.SetBool(AnimIDFreeFall, isFall);
        }

        public void Grounded(bool grounded)
        {
            _animator.SetBool(AnimIDGrounded, grounded);
        }

        public void Hit(int hitType)
        {
            _animator.SetTrigger(AnimIDHit);
            _animator.SetInteger(AnimIDHitInt, hitType);
        }

        public void Reload()
        {
            _animator.SetTrigger(AnimIDReload);
        }

        public void MeleeAttack()
        {
            _animator.SetTrigger(AnimIDMeleeAttack);
        }


        public void Unarmed()
        {
            _animator.SetBool(AnimIDRifle, false);
            _animator.SetBool(AnimIDPistol, false);
            _animator.SetBool(AnimIDNotWeapon, true);
        }

        public void GetPistol()
        {
            _animator.SetBool(AnimIDNotWeapon, false);
            _animator.SetBool(AnimIDRifle, false);
            _animator.SetBool(AnimIDPistol, true);
        }


        public void GetRifle()
        {
            _animator.SetBool(AnimIDPistol, false);
            _animator.SetBool(AnimIDNotWeapon, false);
            _animator.SetBool(AnimIDRifle, true);
        }

        public void Turn(float angle_f, int angle_int)
        {
            _animator.SetFloat(AnimIDTurnAngleFloat, angle_f);
            _animator.SetInteger(AnimIDTurnAngleInt, angle_int);
        }

        public void RigPutRifle()
        {
            _rigAnimator.Play(_stateHashPutRifle);
        }

        public void PlayReloadRig()
        {
            _rigAnimator.Play(_stateHashReloadRig);
        }

        public void RigPutPistol()
        {
            _rigAnimator.Play(_stateHashPutPistol);
        }

        public void RigGetRifle()
        {
            _rigAnimator.Play(_stateHashGetRifle);
        }

        public void RigGetPistol()
        {
            _rigAnimator.Play(_stateHashGetPistol);
        }

        public void RifleShootRecoil()
        {
            _rigAnimator.Play(_stateHashRifleRecoil);
        }

        public void PistolShootRecoil()
        {
            _rigAnimator.Play(_stateHashPistolRecoil);
        }

        public void EnteredState(int stateHash)
        {
            State = StateFor(stateHash);
            StateEntered?.Invoke(State);
        }

        public void ExitedState(int stateHash)
        {
            StateExited?.Invoke(StateFor(stateHash));
        }

        private AnimatorState StateFor(int stateHash)
        {
            AnimatorState state;
            if (stateHash == _stateHashCrouchRifleNotAim || stateHash == _stateHashCrouchRifleAim ||
                stateHash == _stateHashCrouchPistolNotAim || stateHash == _stateHashCrouchPistolAim ||
                stateHash == _stateHashCrouchNotWeapon)
            {
                state = AnimatorState.Sit;
            }
            else if (stateHash == _stateHashWalkPistol || stateHash == _stateHashWalkRifle ||
                     stateHash == _stateHashWalkNotWeapon)
            {
                state = AnimatorState.Walking;
            }
            else if (stateHash == _stateHashPistolAim || stateHash == _stateHashRifleAim)
            {
                state = AnimatorState.Aim;
            }
            else if (stateHash == _stateHashReload)
            {
                state = AnimatorState.Reload;
            }
            else if (stateHash == _stateHashGetPistol || stateHash == _stateHashGetRifle ||
                     stateHash == _stateHashPutPistol || stateHash == _stateHashPutRifle)
            {
                state = AnimatorState.SwitchWeapon;
            }
            else if (stateHash == _stateHashHitStay1 || stateHash == _stateHashHitStay2 ||
                     stateHash == _stateHashHitStay3 || stateHash == _stateHashHitCrouch ||
                     stateHash == _stateHashStartHit)
            {
                state = AnimatorState.Hit;
            }
            else
            {
                state = AnimatorState.Unknown;
            }

            return state;
        }
    }
}