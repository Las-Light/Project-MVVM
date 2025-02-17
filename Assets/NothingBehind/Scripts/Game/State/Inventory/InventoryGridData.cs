using System;
using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class InventoryGridData
    {
        public string GridTypeId;
        public int Width;
        public int Height;
        public bool[] Grid; // Одномерный массив для сериализации
        public List<ItemData> Items; // Данные предметов
        public List<Vector2Int> Positions; // Позиции предметов
    }
}