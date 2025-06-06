using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.FX
{
    public class DestroyFX : MonoBehaviour
    {
        void Start()
        {
            Destroy(gameObject, 0.5f);
        }
    }
}