using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.FSM
{
    public interface FSMState
    {
        void Update (FSM fsm, GameObject gameObject);
    }
}