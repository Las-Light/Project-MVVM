using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class ChangeCoverActions : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _coverChange;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private CoverPointSystem _coverPointSystem;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _coverPointSystem = GetComponent<CoverPointSystem>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public ChangeCoverActions()
        {
            AddPrecondition("needChangeCover", true);
        }


        public override void Reset()
        {
            _coverChange = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _coverChange;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // yes we need to be near a cover
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedCover && _worldData.IsNeedCheckCover && _worldData.IsHaveCover &&
                _worldData.IsNeedChangeCover)
            {
                // проверяет следующее укрытие и если оно свободно и не пройдено то устанавливает его как CurrentCover
                if (_coverPointSystem.CheckNextCoverPoint())
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            // устанавливает CurrentCover как CurrentPatrolPoint
            if (_data.CurrentCover != null)
            {
                _patrolManager.NextPatrolPoint(_data.CurrentCover.Index);
                _worldData.IsFindCover = true;
                _worldData.IsNeedChangeCover = false;
                _worldData.IsHaveCover = false;
                _coverChange = true;
            }

            return _coverChange;
        }
    }
}