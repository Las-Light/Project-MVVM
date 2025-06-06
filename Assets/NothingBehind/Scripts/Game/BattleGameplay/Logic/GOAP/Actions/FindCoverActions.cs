using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class FindCoverActions: GoapAction
    {
        private bool _requiresInRange = false;
        private bool _coverFinded;
        private EnemyWorldData _worldData;
        private CoverPointSystem _coverPointSystem;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _coverPointSystem = GetComponent<CoverPointSystem>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public FindCoverActions()
        {
            AddPrecondition("needCover", true);
        }


        public override void Reset()
        {
            _coverFinded = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _coverFinded;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // yes we need to be near a cover
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedCover && !_worldData.IsNeedCheckCover)
            {
                if (!_worldData.IsTargetLost)
                {
                    Debug.Log("SearchCover");
                    _coverPointSystem.StartSearchCovers();
                }

                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_worldData.IsFindCover)
            {
                _worldData.IsNeedCheckCover = true;
                _worldData.InAction = true;
            }
            else
            {
                _worldData.ResetCoverState();
                _coverPointSystem.StopSearchCovers();
                if (!_worldData.IsTakeShootPos)
                {
                    _worldData.IsNeedShootPos = true;
                }
            }
            _coverFinded = true;
            return true;
        }
    }
}