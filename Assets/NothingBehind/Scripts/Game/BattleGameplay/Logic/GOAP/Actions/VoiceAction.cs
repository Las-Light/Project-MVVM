using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Data;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Actions
{
    public class VoiceAction : GoapAction
    {
        private bool _requiresInRange = false;
        private bool _voiceState;
        private EnemyWorldData _worldData;
        private EnemyMovementController _controller;

        private void Awake()
        {
            _worldData = GetComponent<EnemyWorldData>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public VoiceAction()
        {
            AddPrecondition("voice", true);
            AddEffect("voice", false);
        }

        public override void Reset()
        {
            _voiceState = false;
            Target = null;
        }

        public override bool IsDone()
        {
            return _voiceState;
        }

        public override bool RequiresInRange()
        {
            return _requiresInRange;
        }

        public override bool CheckProceduralPrecondition(GameObject agent)
        {
            if (_worldData.IsNeedVoice)
            {
                return true;
            }

            return false;
        }

        public override bool Perform(GameObject agent)
        {
            if (_controller.Voice())
            {
                _worldData.IsNeedVoice = false;
                _voiceState = true;
                return true;
            }

            return false;
        }
    }
}