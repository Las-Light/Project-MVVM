using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "CharacterLevelSettings", menuName = "Game Settings/Characters/New Character Level Settings")]
    public class CharacterLevelSettings : ScriptableObject
    {
        public int Level;
        public float Health;
    }
}