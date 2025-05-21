using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace Code.GOAP.Actions
{
    public class PatrolAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _bisPatrolling = false;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _patrolManager = GetComponent<PatrolManager>();
            _worldData = GetComponent<EnemyWorldData>();
            _data = GetComponent<EnemyData>();
        }

        public PatrolAction()
        {
            AddPrecondition("hasNeedPatrol", true);
            AddEffect("secureArea", true);
        }

        public override void Reset()
        {
            _bisPatrolling = false;
            _worldData.IsMove = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _bisPatrolling;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (!_worldData.IsNeedPatrol)
            {
                return false;
            }

            Target = _patrolManager.GetCurrentPatrolPoint();
            if (Target != null)
            {
                _data.CurrentMovementPos = Target.transform.position;
                _worldData.IsMove = true;
            }

            return Target != null;
        }

        public override bool Perform(GameObject agent)
        {
            _bisPatrolling = true;
            _patrolManager.NextPatrolPoint();
            _worldData.IsMove = false;
            return true;
        }
    }
}