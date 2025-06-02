using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class MoveToInvestPointAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _pointClosely;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public MoveToInvestPointAction()
        {
            AddPrecondition("findInvestPoint", true); // if we have cover we don't want more
        }


        public override void Reset()
        {
            _pointClosely = false;
            _worldData.IsMove = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _pointClosely;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsFindInvestPoint)
            {
                Target = _patrolManager.GetCurrentPatrolPoint();
                if (Target != null && Target.transform.position != Vector3.zero)
                {
                    _data.CurrentMovementPos = Target.transform.position;
                    _worldData.ResetStayState();
                    _worldData.IsMove = true;
                    return true;
                }
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (!_patrolManager.PatrolPointList[_patrolManager.CurrentPatrolPoint.NextIndex].HasBeenPassed)
            {
                _patrolManager.NextPatrolPoint();
            }
            else
            {
                _worldData.InAction = false;
                _worldData.ResetInvestigationState();
                _patrolManager.StopInvestigationAction();
                _worldData.IsInvestigationEnd = true;
                _worldData.IsMove = false;
                GlobalEventManager.SendUpdatePosition(gameObject);
            }

            _pointClosely = true;
            return true;
        }
    }
}