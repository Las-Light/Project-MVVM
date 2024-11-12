using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings/New Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public CharactersSettings CharactersSettings;
    }
}