using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.Actions
{
    public class MoveToCheckAreaAction : GoapAction
    {
        private bool _requiresInRange = true;
        private bool _moveToCheckArea;
        private EnemyWorldData _worldData;
        private EnemyData _data;
        private PatrolManager _patrolManager;

        private void Awake()
        {
            _data = GetComponent<EnemyData>();
            _worldData = GetComponent<EnemyWorldData>();
            _patrolManager = GetComponent<PatrolManager>();
        }

        public MoveToCheckAreaAction()
        {
            AddPrecondition("moveToCheckArea", true); // if we have cover we don't want more
        }


        public override void Reset()
        {
            _moveToCheckArea = false;
            _worldData.IsMove = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _moveToCheckArea;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsFindPosCheckArea)
            {
                Target = _patrolManager.GetCurrentPatrolPoint();
                if (Target != null && Target.transform.position != Vector3.zero)
                {
                    _data.CurrentMovementPos = Target.transform.position;
                    _worldData.ResetStayState();
                    _worldData.IsMove = true;
                    return true;
                }
                _worldData.IsNeedCheckArea = true;
                _worldData.IsFindPosCheckArea = false;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            Debug.Log("FindCheckAreaFalse " + gameObject.name);
            _worldData.IsFindPosCheckArea = false;
            _worldData.IsNeedVoice = true;
            _worldData.IsCheckArea = true;
            _worldData.IsNeedRotate = true;
            _worldData.IsFoundAboutTarget = false;
            _worldData.IsMove = false;
            _worldData.InAction = false;
            _patrolManager.StopCheckAreaAction();
            
            //GlobalEventManager.SendUpdatePosition(gameObject);

            _moveToCheckArea = true;
            return true;
        }
    }
}