using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.FSM
{
    public interface FSMState
    {
        void Update (FSM fsm, GameObject gameObject);
    }
}