using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class MoveToPositionAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _moveComplete;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private EnemyMovementController _controller;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public MoveToPositionAction()
        {
            AddPrecondition("needMoveShootPos", true); // if we have cover we don't want more
        }


        public override void Reset()
        {
            _moveComplete = false;
            _worldData.IgnoreTarget = false; // на случай если действие прервется то надо сбросить IsMove
            _worldData.IsMove = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _moveComplete;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsFindFireLine || _worldData.IsFindShootPos)
            {
                Target = _patrolManager.GetCurrentPatrolPoint();

                if (Target != null && Target.transform.position != Vector3.zero)
                {
                    _data.CurrentMovementPos = Target.transform.position;
                    _worldData.ResetStayState();
                    _worldData.IgnoreTarget = true;
                    _worldData.IsMove = true;
                    _worldData.IsAim = true;
                    return true;
                }

                _worldData.ResetMoveState();
                _patrolManager.StopMoveAction();
                return false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _worldData.IsFindFireLine = false;
            _worldData.IsFindShootPos = false;
            _worldData.IsNeedShootNow = false;
            _worldData.IsNeedMoveBack = false;
            _worldData.IsTakeShootPos = true;
            _worldData.IsNeedRotate = true;
            _worldData.IgnoreTarget = false;
            _worldData.IsMove = false;
            _worldData.InAction = false;

            _moveComplete = true;
            //GlobalEventManager.SendUpdatePosition(gameObject);
            return true;
        }
    }
}