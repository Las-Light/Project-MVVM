using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class MoveToEnemyAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _enemyClosely;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public MoveToEnemyAction()
        {
            AddPrecondition("needClosely", true); // if we have cover we don't want more
            AddEffect("enemyClosely", true);
        }


        public override void Reset()
        {
            _enemyClosely = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _enemyClosely;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedClosely && _worldData.IsSeeEnemy)
            {
                Target = _data.CurrentEnemy;
                return true;
            }

            _worldData.IsNeedClosely = false;

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _worldData.IsNeedClosely = false;
            _enemyClosely = true;
            return true;
        }
    }
}