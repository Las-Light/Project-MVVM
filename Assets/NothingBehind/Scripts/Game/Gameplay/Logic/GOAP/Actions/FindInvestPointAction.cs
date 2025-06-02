using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.EventManager;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class FindInvestPointAction : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _investigateEnd = false;

        private InvestigatePointSystem _investigateSystem;
        private PatrolManager _patrolManager;
        private EnemyWorldData _worldData;


        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _investigateSystem = GetComponent<InvestigatePointSystem>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public FindInvestPointAction()
        {
            AddPrecondition("shouldBeInvestigating", true);
        }

        public override void Reset()
        {
            _investigateEnd = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _investigateEnd;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedInvestigation)
            {
                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            Debug.Log("StartFindInvest " + gameObject.name);
            _investigateSystem.StartSearchInvestigationPoints();
            if (_worldData.IsFindInvestPoint)
            {
                _investigateEnd = true;
                _worldData.IsNeedInvestigation = false;
                _worldData.IsHearingSound = false;
                _worldData.InAction = true;
                Debug.Log("FindInvest " + gameObject.name);
            }
            else
            {
                _worldData.ResetInvestigationState();
                _patrolManager.StopInvestigationAction();
                _worldData.IsInvestigationEnd = true;
                _investigateEnd = false;
                GlobalEventManager.SendUpdatePosition(gameObject);
                Debug.Log("NotFindInvest " + gameObject.name);
            }

            return _investigateEnd;
        }
    }
}