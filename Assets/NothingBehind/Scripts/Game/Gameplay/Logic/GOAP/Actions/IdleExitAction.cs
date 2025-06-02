using NothingBehind.Scripts.Game.Gameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class IdleExitAction: GoapAction
    {
        private bool _requiresInRange = false;
        private bool _idleStateExit;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _data = GetComponent<EnemyData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public IdleExitAction()
        {
            AddEffect("idleStateExit", true);
        }
        public override void Reset()
        {
            _idleStateExit = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _idleStateExit;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }
        
        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsIdleStateExit)
            {
                return true;
            }
            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _controller.IdleStateExit();
            _worldData.IsIdleStateExit = false;
            _idleStateExit = true;
            return true;
        }

    }
}