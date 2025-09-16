using NothingBehind.Scripts.Game.Settings.Gameplay;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Player;
using NothingBehind.Scripts.Game.Settings.Gameplay.Equipment;
using NothingBehind.Scripts.Game.Settings.Gameplay.Inventory;
using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.Settings.GlobalMap;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings/New Game Settings")]
    public class GameSettings : ScriptableObject
    {
        public EntitiesSettings EntitiesSettings;
        public InventoriesSettings InventoriesSettings;
        public ItemsSettings ItemsSettings;
        public WeaponsSettings WeaponsSettings;
        public EquipmentsSettings EquipmentsSettings;
        public GameplayCameraSettings GameplayCameraSettings;
        public MapsSettings MapsSettings;
        public GlobalMapSettings GlobalMapSettings;
    }
}