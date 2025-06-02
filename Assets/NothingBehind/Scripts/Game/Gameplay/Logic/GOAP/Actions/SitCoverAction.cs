using NothingBehind.Scripts.Game.Gameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class SitCoverActions : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _sitDone;
        private EnemyWorldData _enemyWorldData;
        private EnemyData _data;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _enemyWorldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public SitCoverActions()
        {
            AddPrecondition("needSit", true);
        }


        public override void Reset()
        {
            _sitDone = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _sitDone;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // yes we need to be near a cover
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_enemyWorldData.IsNeedSit)
            {
                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_controller.Sit())
            {
                _enemyWorldData.IsNeedRotate = true;
                _enemyWorldData.IsNeedSit = false;
                //GlobalEventManager.SendUpdatePosition(gameObject);
                _sitDone = true;
            }
            return _sitDone;
        }
    }
}
