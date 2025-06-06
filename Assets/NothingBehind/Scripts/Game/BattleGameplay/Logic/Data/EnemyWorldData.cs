using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.Data
{
    public class EnemyWorldData: MonoBehaviour
    {
        public BaseRole BaseRole { get; set; }
        public AttackRole AttackRole { get; set; }
        
        public bool IsAttack { get; set; }
        public bool IsNeedShootNow;
        public bool IsNeedCover;
        public bool IsSeeEnemy;
        public bool IsCouldSeeEnemy;
        public bool IsTargetLost;
        public bool IsCanAttack;
        public bool IsNeedAttack;
        public bool IsNeedInvestigation;
        public bool IsHaveCover;
        public bool IsNeedClosely;
        public bool IsAttacking;
        public bool IsNeedVoice;
        public bool IsNeedWait;
        public bool IsHearingSound;
        public bool IsFindCover;
        public bool IsNeedCheckCover;
        public bool IsNeedChangeCover;
        public bool IsCoverPointPassed;
        public bool IsIgnoreCoverPassed;
        public bool IsFindPosCheckArea;
        public bool IsFindInvestPoint;
        public bool IsNeedFireLine;
        public bool IsFindFireLine;
        public bool IsNeedShootPos;
        public bool IsFindShootPos;
        public bool IsNeedMoveBack ;
        public bool IsCheckArea;
        public bool IsInvestigationEnd;
        public bool IsTakeShootPos;
        public bool IsNeedFlank;
        public bool IsNeedSit;
        public bool IsNeedAbortWait;
        public bool IsNeedStay;
        public bool IsNeedRotate;
        public bool IsJoinBattle;
        public bool IsNeedPatrol;
        public bool IsNeedCheckArea;
        
        public bool IsTargetClosely => _data.SqrtDistanceToTarget < 2;


        public bool IsAimAnimation => _animator.State == AnimatorState.Aim;

        public bool InAction; // переменная нужна для того чтобы постоянно не срабатывал SetRole в CombatController
        public bool IsSit;
        public bool IsAim;
        public bool IsMove; // переменная нужна для того чтобы Interrupt срабатывал только когда юнит движется
        public bool IgnoreTarget; // переменная нужна для того чтобы когда она true IsTargetLost не срабатывал
        public bool IsFoundAboutTarget;
        public bool IsIdleState;
        public bool IsIdleStateExit;

        private EnemyData _data;
        private AnimatorController _animator;

        private void Start()
        {
            _data = GetComponent<EnemyData>();
            _animator = GetComponent<AnimatorController>();
            SetStateFromRole();
        }

        private void Update()
        {
            SetIsAttacking();
        }

        public void SetStateFromRole()
        {
            switch (BaseRole)
            {
                case BaseRole.Approacher:
                    InAction = false;
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedClosely = true;
                    break;
                case BaseRole.CheckFlank:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetMoveState();
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedCheckArea = true;
                    IsNeedFlank = true;
                    break;
                case BaseRole.CheckFront:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetMoveState();
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedCheckArea = true;
                    IsAim = true;
                    break;
                case BaseRole.FoundAboutTarget:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetMoveState();
                    ResetCheckAreaState();
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedCheckArea = true;
                    IsAim = false;
                    break;
                case BaseRole.HearingSound:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetCheckAreaState();
                    ResetMoveState();
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedCheckArea = true;
                    IsAim = true;
                    break;
                case BaseRole.Shooter:
                    InAction = false;
                    IsNeedPatrol = false;
                    IsNeedInvestigation = false;
                    IsNeedWait = false;
                    if (_data.SqrtDistanceToTarget is <= 50 and > 25 && !IsTakeShootPos)
                    {
                        IsNeedShootPos = true;
                    }
                    if (_data.SqrtDistanceToTarget is <= 25 and > 10 && !IsTakeShootPos)
                    {
                        IsNeedShootPos = true;
                        IsNeedShootNow = true;
                    }
                    if (_data.SqrtDistanceToTarget < 10 && !IsTakeShootPos)
                    {
                        IsNeedShootPos = true;
                        IsNeedMoveBack = true;
                    }
                    
                    else if (!IsTakeShootPos && !IsNeedShootPos) 
                    {
                        IsNeedCover = true;
                    }

                    break;
                case BaseRole.StayUpAndAimer:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetMoveState();
                    ResetCheckAreaState();
                    InAction = true;
                    IsNeedPatrol = false;
                    IsNeedWait = true;
                    IsNeedStay = true;
                    break;
                case BaseRole.Investigator:
                    ResetCoverState();
                    ResetMoveState();
                    ResetCheckAreaState();
                    IsNeedPatrol = false;
                    IsNeedWait = false;
                    IsNeedInvestigation = true;
                    IsAim = true;
                    break;
                case BaseRole.Patrol:
                    ResetCoverState();
                    ResetInvestigationState();
                    ResetMoveState();
                    ResetCheckAreaState();
                    IsInvestigationEnd = false;
                    InAction = false;
                    IsNeedWait = false;
                    IsNeedPatrol = true;
                    Debug.Log("PatrolRole");
                    break;
            }
        }

        public void SetStateFromQuery()
        {
            switch (AttackRole)
            {
                case AttackRole.MeleeFirst:
                    IsJoinBattle = true;
                    IsCanAttack = true;
                    IsNeedWait = false;
                    break;
                case AttackRole.ShooterFirst:
                    IsJoinBattle = true;
                    IsCanAttack = true;
                    IsNeedWait = false;
                    IsNeedStay = false;
                    break;
                case AttackRole.MeleeSecond:
                    IsJoinBattle = true;
                    IsCanAttack = false;
                    break;
                case AttackRole.ShooterSecond:
                    IsJoinBattle = true;
                    IsCanAttack = false;
                    IsNeedStay = false;
                    if (_data.SqrtDistanceToTarget >= 100 && !IgnoreTarget && IsHaveCover)
                    {
                        IsNeedWait = false;
                        IsNeedChangeCover = true;
                    }

                    if ((IsNeedChangeCover && IsCoverPointPassed) || !IsNeedCover || !IsNeedChangeCover)
                    {
                        IsNeedWait = true;
                    }

                    break;
                case AttackRole.ShooterStay:
                    IsJoinBattle = true;
                    IsCanAttack = true;
                    IsNeedWait = false;
                    IsNeedStay = true;
                    break;
                case AttackRole.Noncombatant:
                    IsJoinBattle = false;
                    IsCanAttack = false;
                    break;
            }
        }

        public void ResetMoveState()
        {
            IsNeedFireLine = false;
            IsFindFireLine = false;
            IsNeedShootPos = false;
            IsFindShootPos = false;
        }

        public void ResetInvestigationState()
        {
            IsNeedInvestigation = false;
            IsFindInvestPoint = false;
        }

        public void ResetCheckAreaState()
        {
            IsNeedFlank = false;
            IsCheckArea = false;
            IsFindPosCheckArea = false;
            IsNeedCheckArea = false;
        }

        public void ResetCoverState()
        {
            IsNeedCover = false;
            IsFindCover = false;
            IsHaveCover = false;
            IsNeedCheckCover = false;
            IsNeedChangeCover = false;
            IsCoverPointPassed = false;
            IsIgnoreCoverPassed = false;
        }

        public void ResetStayState()
        {
            IsNeedStay = false;
            IsTakeShootPos = false;
        }

        private void SetIsAttacking()
        {
            if (IsSeeEnemy)
            {
                IsAttacking = false;
            }
        }
    }
}