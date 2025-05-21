using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using UnityEngine;

namespace Code.GOAP.Actions
{
    public class FirearmsAttackActions : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _targetDamage;

        private EnemyWorldData _worldData;
        private EnemyData _data;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public FirearmsAttackActions()
        {
            AddPrecondition("canAttack", true); // we have enemy
            AddEffect("enemyDamage", true);
        }


        public override void Reset()
        {
            _targetDamage = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _targetDamage;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange; // no we not need to be near a target
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedAttack)
            {
                if (!_worldData.IsJoinBattle)
                {
                    GlobalEventManager.SendAttackTarget(gameObject);
                }

                if (_worldData.IsCanAttack)
                {
                    if (_data.CurrentEnemy != null)
                    {
                        Target = _data.CurrentEnemy;
                        return true;
                    }
                }
                
                return false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (Target)
            {
                if (_data.SqrtDistanceToTarget > 100 && _worldData.IsTakeShootPos || _worldData.IsCheckArea)
                {
                    _worldData.IsTakeShootPos = false;
                    _worldData.IsCheckArea = false;
                    GlobalEventManager.SendUpdatePosition(gameObject);
                }
                if (_data.SqrtDistanceToTarget > 25 && _data.SqrtDistanceToTarget <= 100)
                {
                    _worldData.IsNeedStay = true;
                }

                if (_data.SqrtDistanceToTarget < 10 && !_worldData.IsTakeShootPos)
                {
                    _worldData.IsNeedShootPos = true;
                    _worldData.IsNeedMoveBack = true;
                }
                
                if (_controller.FirearmsAttackTarget(Target))
                {
                    if (_worldData.IsHaveCover && !_worldData.IsNeedStay)
                    {
                        _worldData.IsNeedSit = true;
                    }
                    _targetDamage = true;
                }
            }

            return _targetDamage;
        }
    }
}