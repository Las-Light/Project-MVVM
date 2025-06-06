using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class IdleAction: GoapAction
    {
        private bool _requiresInRange = false;
        private bool _idleState;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _data = GetComponent<EnemyData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public IdleAction()
        {
            AddEffect("idleState", true);
        }
        public override void Reset()
        {
            _idleState = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _idleState;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }
        
        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsIdleState)
            {
                return true;
            }
            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _controller.IdleState();
            
            _idleState = true;
            return true;
        }
    }
}