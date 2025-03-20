using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Inventory Config", menuName = "Inventory/Inventory Config", order = 1)]
    public class InventorySettings : ScriptableObject
    {
        public EntityType OwnerType;
        public List<InventoryGridSettings> GridsSettings;
    }
}