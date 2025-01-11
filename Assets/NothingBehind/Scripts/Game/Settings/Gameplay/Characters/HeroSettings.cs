using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "HeroSettings", menuName = "Game Settings/Characters/New Hero Settings")]
    public class HeroSettings : ScriptableObject
    {
        public float Health;
    }
}