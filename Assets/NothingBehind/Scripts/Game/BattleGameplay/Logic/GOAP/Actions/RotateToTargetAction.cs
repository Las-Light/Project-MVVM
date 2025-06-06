using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class RotateToTargetAction : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _rotateDone;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public RotateToTargetAction()
        {
            AddPrecondition("needRotate", true); // if we have cover we don't want more
        }


        public override void Reset()
        {
            _rotateDone = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _rotateDone;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedRotate)
            {
                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_data.CurrentEnemy)
            {
                if (_controller.RotateToTarget(_data.CurrentEnemy.transform.position))
                {
                    _worldData.IsNeedRotate = false;
                    GlobalEventManager.SendUpdatePosition(gameObject);
                    _rotateDone = true;
                    return true;
                }
            }
            else if (_worldData.IsHearingSound && _data.HeardSoundPosition != Vector3.zero)
            {
                if (_controller.RotateToTarget(_data.HeardSoundPosition))
                {
                    _worldData.IsNeedRotate = false;
                    GlobalEventManager.SendUpdatePosition(gameObject);
                    _rotateDone = true;
                    return true;
                }
            }

            return false;
        }
    }
}