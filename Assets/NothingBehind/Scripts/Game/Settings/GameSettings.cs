using NothingBehind.Scripts.Game.Settings.Gameplay;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings/New Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public HeroSettings HeroSettings;
        public CharactersSettings CharactersSettings;
        public InventoriesSettings InventoriesSettings;
        public GameplayCameraSettings GameplayCameraSettings;
        public MapsSettings MapsSettings;
    }
}