using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class FindPosForCheckAreaAction : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _checkArea;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public FindPosForCheckAreaAction()
        {
            AddPrecondition("needCheckArea", true); // if we have cover we don't want more
        }


        public override void Reset()
        {
            _checkArea = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _checkArea;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedCheckArea && !_worldData.IsFindPosCheckArea)
            {
                return true;
            }

            if (_worldData.IsNeedCheckArea && _worldData.IsFoundAboutTarget)
            {
                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_data.SoundDetectionTime > _data.TargetLastKnownDetectionTime && !_worldData.IsFoundAboutTarget)
            {
                if (_worldData.IsHearingSound && _data.HeardSoundPosition != Vector3.zero)
                {
                    if (_worldData.IsNeedFlank)
                    {
                        // если точки с фланга не нашлись то идет проверять саму позицию
                        if (!_patrolManager.GetFlankPosition(_data.HeardSoundPosition))
                            _patrolManager.CreateRouteUseRandomPoints(_data.HeardSoundPosition, 1, 1f);

                        _worldData.IsNeedFlank = false;
                    }
                    else
                        _patrolManager.CreateRouteUseRandomPoints(_data.HeardSoundPosition, 1, 1f);
                }
                else if (_data.TargetLastKnownPosition != Vector3.zero)
                    _patrolManager.CreateRouteUseRandomPoints(_data.TargetLastKnownPosition, 1, 1f);
            }

            else if (_worldData.IsTargetLost && _data.TargetLastKnownPosition != Vector3.zero &&
                     !_worldData.IsFoundAboutTarget)
            {
                if (_worldData.IsNeedFlank)
                {
                    // если и справа не нашлись то идет проверять последнюю позицию игрока
                    if (!_patrolManager.GetFlankPosition(_data.TargetLastKnownPosition))
                        _patrolManager.CreateRouteUseRandomPoints(_data.TargetLastKnownPosition, 1, 1f);

                    _worldData.IsNeedFlank = false;
                }
                else
                    _patrolManager.CreateRouteUseRandomPoints(_data.TargetLastKnownPosition, 1, 1f);
            }

            if (_worldData.IsFoundAboutTarget && _data.TargetLastKnownPosition != Vector3.zero)
            {
                if (!_patrolManager.GetFlankPosition(_data.TargetLastKnownPosition))
                    _patrolManager.CreateRouteUseRandomPoints(_data.TargetLastKnownPosition, 1, 1f);
            }

            _worldData.IsNeedCheckArea = false;
            _worldData.IsFindPosCheckArea = true;

            _worldData.InAction = true;

            _checkArea = true;
            return true;
        }
    }
}