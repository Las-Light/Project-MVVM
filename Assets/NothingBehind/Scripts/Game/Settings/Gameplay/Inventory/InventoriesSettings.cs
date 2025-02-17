using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Inventories Config", menuName = "Inventory/Inventories Config", order = 0)]
    public class InventoriesSettings : ScriptableObject
    {
        public List<InventorySettings> Inventories;
    }
}