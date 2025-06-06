using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class FindPosForShootActions : GoapAction
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


        public FindPosForShootActions()
        {
            AddPrecondition("needFindShootPos", true);
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
            if (_worldData.IsNeedShootPos)
            {
                if (!_worldData.IsHaveCover)
                {
                    return true;
                }

                _worldData.ResetMoveState();
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_worldData.BaseRole != BaseRole.Investigator)
            {
                if (_worldData.IsNeedShootNow)
                    _patrolManager.CreateSelfPoint(transform.position);
                else if (_worldData.IsNeedMoveBack)
                {
                    // если юнит слишком близко к игроку то ищет позицию для отхода
                    if (!_patrolManager.CreateRandomShootPoint(_patrolManager.GetShootBackPoint(
                            _data.CurrentEnemy.transform.position, transform.position,
                            _data.DistanceToTarget), _data.DistanceToTarget / 2))
                    {
                        _patrolManager.CreateSelfPoint(transform.position);
                    }
                }
                else
                {
                    // если нет укрытий и расстояние до игрока далеко, то юнит сближается с игроком
                    _patrolManager.CreateRandomShootPoint(_patrolManager.GetShootFrontPoint(
                        _data.CurrentEnemy.transform.position, transform.position,
                        _data.DistanceToTarget), _data.DistanceToTarget / 2);
                }

                _worldData.InAction = true;
                _worldData.IsNeedShootPos = false;
                _worldData.IsFindShootPos = true;
                _posFinded = true;
            }
            else
            {
                _worldData.ResetMoveState();
                _patrolManager.StopMoveAction();
            }

            return _posFinded;
        }
    }
}