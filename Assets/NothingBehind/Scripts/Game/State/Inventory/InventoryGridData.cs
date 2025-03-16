using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    [Serializable]
    public class InventoryGridData
    {
        public int OwnerId;
        public string GridTypeId;
        public int Width;
        public int Height;
        public float CellSize;
        public bool IsSubGrid;
        public List<InventoryGridData> SubGrids;
        public bool[] Grid; // Одномерный массив для сериализации
        public List<ItemData> Items; // Данные предметов
        public List<Vector2Int> Positions; // Позиции предметов

        public InventoryGridData(int ownerId,
            string gridTypeId, 
            int width, 
            int height, 
            float cellSize, 
            bool isSubGrid,
            List<InventoryGridData> subGrids,
            List<ItemData> items)
        {
            OwnerId = ownerId;
            GridTypeId = gridTypeId;
            Width = width;
            Height = height;
            CellSize = cellSize;
            IsSubGrid = isSubGrid;
            SubGrids = subGrids;
            Positions = new List<Vector2Int>();
            Items = items;
            Grid = new bool[Height * Width];
            
            foreach (var itemData in items)
            {
                var position = FindFreePosition(itemData);
                if (position.HasValue)
                {
                    PlaceItem(itemData, position.Value, itemData.IsRotated);
                    Positions.Add(position.Value);
                }
            }
        }

        private Vector2Int? FindFreePosition(ItemData item)
        {
            for (int y = 0; y < Height - item.Height + 1; y++)
            {
                for (int x = 0; x < Width - item.Width + 1; x++)
                {
                    if (CanPlaceItem(item, new Vector2Int(x, y), isRotated: false))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }
    
        private void PlaceItem(ItemData item, Vector2Int position, bool isRotated)
        {
            if (!CanPlaceItem(item, position, isRotated))
                return;

            int itemWidth = isRotated ? item.Height : item.Width;
            int itemHeight = isRotated ? item.Width : item.Height;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    Grid[i * Height + j] = true;
                }
            }
        }

        private bool CanPlaceItem(ItemData item, Vector2Int position, bool isRotated)
        {
            int itemWidth = isRotated ? item.Height : item.Width;
            int itemHeight = isRotated ? item.Width : item.Height;

            if (position.x < 0 || position.y < 0 || position.x + itemWidth > Width ||
                position.y + itemHeight > Height)
                return false;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    if (Grid[i * Height + j]) return false;
                }
            }

            return true;
        }
    }
}