using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventory;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Grid Config", menuName = "Inventory/Grid Config", order = 2)]
    public class InventoryGridSettings : ScriptableObject
    {
        public string GridTypeId;
        public int Width;
        public int Height;
    }
}