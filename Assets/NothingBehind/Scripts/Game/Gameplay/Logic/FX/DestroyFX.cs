using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.FX
{
    public class DestroyFX : MonoBehaviour
    {
        void Start()
        {
            Destroy(gameObject, 0.5f);
        }
    }
}