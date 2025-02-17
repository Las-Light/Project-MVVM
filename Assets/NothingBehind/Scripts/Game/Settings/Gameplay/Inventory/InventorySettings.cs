using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Inventory Config", menuName = "Inventory/Inventory Config", order = 1)]
    public class InventorySettings : ScriptableObject
    {
        public string OwnerTypeId;
        public List<InventoryGridSettings> InventoryGrids;
    }
}