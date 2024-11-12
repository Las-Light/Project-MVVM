using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Characters
{
    public class CharacterBinder : MonoBehaviour
    {
        public void Bind(CharacterViewModel viewModel)
        {
            transform.position = viewModel.Position.CurrentValue;
        }
    }
}