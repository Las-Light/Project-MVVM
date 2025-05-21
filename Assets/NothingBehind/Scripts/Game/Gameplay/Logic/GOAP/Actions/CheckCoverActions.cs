using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class CheckCoverActions: GoapAction
    {
        private bool _requiresInRange = false;
        private bool _coverCheck;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private CoverPointSystem _coverPointSystem;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _coverPointSystem = GetComponent<CoverPointSystem>();
        }

        public CheckCoverActions()
        {
            AddPrecondition("needCover", true);
        }


        public override void Reset()
        {
            _coverCheck = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _coverCheck;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // yes we need to be near a cover
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedCover && _worldData.IsNeedCheckCover && _worldData.IsHaveCover)
            {
                if (_data.CurrentCover != null)
                {
                    if (_worldData.IsTargetLost && !_worldData.IsNeedStay)
                    {
                        _worldData.ResetCoverState();
                        _coverPointSystem.StopSearchCovers();
                    }
                    if (_coverPointSystem.CheckCoverByDot())
                        return true;

                    return false;
                }
                return false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _worldData.IsNeedCheckCover = false;
            _worldData.IsHaveCover = false;
            _coverCheck = true;
            return true;
        }
    }
}