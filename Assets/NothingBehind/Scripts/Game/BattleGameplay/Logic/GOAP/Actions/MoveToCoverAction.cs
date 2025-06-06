using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class MoveToCoverAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _coverClosely;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public MoveToCoverAction()
        {
            AddPrecondition("findCover", true); // if we have cover we don't want more
            AddEffect("hasCover", true);
        }


        public override void Reset()
        {
            _coverClosely = false;
            _worldData.IgnoreTarget = false; // на случай если действие прервется то надо сбросить IsMove
            _worldData.IsMove = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _coverClosely;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (!_worldData.IsHaveCover && _worldData.IsFindCover)
            {
                if (_data.CurrentCover != null)
                {
                    // проверка на удаленность укрытия, если далеко то не убирать прицеливание, если близко то убрать
                    if ((transform.position - _data.CurrentCover.PointPosition).sqrMagnitude >= 4)
                        _worldData.IsAim = false;
                    else
                        _worldData.IsAim = true;
                    
                    Target = _patrolManager.GetCurrentPatrolPoint();
                    
                    if (Target != null &&
                        (_data.CurrentCover.PointPosition - Target.transform.position).sqrMagnitude < 1 &&
                        Target.transform.position != Vector3.zero)
                    {
                        _data.CurrentMovementPos = Target.transform.position;
                        _worldData.ResetStayState();
                        _worldData.IsMove = true;
                        _worldData.IgnoreTarget = true;
                        return true;
                    }

                    _worldData.IsFindCover = false;
                    _worldData.IsNeedCheckCover = false;
                    return false;
                }

                _worldData.IsFindCover = false;
                _worldData.IsNeedCheckCover = false;
                return false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            _worldData.IsHaveCover = true;
            _worldData.IsFindCover = false;
            _worldData.IsNeedRotate = true;
            _worldData.IgnoreTarget = false;
            _worldData.IsMove = false;
            _worldData.InAction = false;

            _coverClosely = true;
            //GlobalEventManager.SendUpdatePosition(gameObject);
            return true;
        }
    }
}