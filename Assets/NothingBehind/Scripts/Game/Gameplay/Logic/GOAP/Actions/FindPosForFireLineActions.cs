using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class FindPosForFireLineActions : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _posFinded;
        private EnemyData _data;
        private EnemyWorldData _worldData;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }


        public FindPosForFireLineActions()
        {
            AddPrecondition("needFireLine", true);
        }


        public override void Reset()
        {
            _posFinded = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _posFinded;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // no we not need to be near a target
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedFireLine)
            {
                if (!_worldData.IsHaveCover)
                {
                    return true;
                }

                // если юниту закрыт обзор на игрока когда он в укрытии он ищет новое укрытие
                _worldData.IsIgnoreCoverPassed = true;
                _worldData.IsNeedChangeCover = true;
                _worldData.ResetMoveState();
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (!_patrolManager.CreateRouteForMoveToSide(gameObject.transform))
            {
                _patrolManager.CreateRandomShootPoint(_patrolManager.GetShootFrontPoint(
                    _data.CurrentEnemy.transform.position, transform.position,
                    _data.DistanceToTarget), _data.DistanceToTarget / 2);
            }

            _worldData.InAction = true;
            _worldData.IsNeedFireLine = false;
            _worldData.IsFindFireLine = true;

            _posFinded = true;
            return true;
        }
    }
}