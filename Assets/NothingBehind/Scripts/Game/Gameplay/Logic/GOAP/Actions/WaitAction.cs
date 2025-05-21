using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using UnityEngine;

namespace Code.GOAP.Actions
{
    public class WaitAction : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _waitState;
        private EnemyWorldData _worldData;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public WaitAction()
        {
            AddPrecondition("wait", true);
        }

        public override void Reset()
        {
            _waitState = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _waitState;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedWait)
            {
                return true;
            }
            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _controller.Wait();
            _waitState = true;
            return true;
        }
    }
}