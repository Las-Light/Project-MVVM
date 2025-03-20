using NothingBehind.Scripts.Game.Settings.Equipment;
using NothingBehind.Scripts.Game.Settings.Gameplay;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings/New Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public PlayerSettings PlayerSettings;
        public CharactersSettings CharactersSettings;
        public InventoriesSettings InventoriesSettings;
        public ItemsSettings ItemsSettings;
        public EquipmentsSettings EquipmentsSettings;
        public GameplayCameraSettings GameplayCameraSettings;
        public MapsSettings MapsSettings;
    }
}