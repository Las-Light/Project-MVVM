using NothingBehind.Scripts.Game.Settings.Gameplay;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Equipment;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.Settings.Gameplay.Storages;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.Settings.GlobalMap;
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
        public WeaponsSettings WeaponsSettings;
        public EquipmentsSettings EquipmentsSettings;
        public StoragesSettings StoragesSettings;
        public GameplayCameraSettings GameplayCameraSettings;
        public MapsSettings MapsSettings;
        public GlobalMapSettings GlobalMapSettings;
    }
}