using System.Linq;
using NothingBehind.Scripts.Game.State.Items;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventories.Grids
{
    public class InventoryGrid
    {
        public int GridId { get; }
        public InventoryGridType GridType { get; }
        public InventoryGridData Origin { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public bool IsSubGrid { get; }
        
        public readonly ObservableDictionary<Item, Vector2Int> ItemsPositionsMap = new();
        public ReactiveProperty<bool[]> Grid { get; } // Одномерный массив для сериализации


        public InventoryGrid(InventoryGridData inventoryGridData)
        {
            Origin = inventoryGridData;
            GridId = inventoryGridData.GridId;
            GridType = inventoryGridData.GridType;
            Width = inventoryGridData.Width;
            Height = inventoryGridData.Height;
            CellSize = inventoryGridData.CellSize;
            IsSubGrid = inventoryGridData.IsSubGrid;
            Grid = new ReactiveProperty<bool[]>(inventoryGridData.Grid);
            for (int i = 0; i < inventoryGridData.Items.Count; i++)
            {
                ItemsPositionsMap[ItemsFactory.CreateItem(inventoryGridData.Items[i])] = inventoryGridData.Positions[i];
            }

            Grid.Subscribe(value => inventoryGridData.Grid = value);

            ItemsPositionsMap.ObserveAdd().Subscribe(kvp =>
            {
                var addedItemData = kvp.Value.Key;
                var addedPosition = kvp.Value.Value;
                inventoryGridData.Items.Add(addedItemData.Origin);
                inventoryGridData.Positions.Add(addedPosition);
            });
            ItemsPositionsMap.ObserveRemove().Subscribe(kvp =>
            {
                var removedItem = kvp.Value.Key;
                var removedPosition = kvp.Value.Value;
                var removedItemData = inventoryGridData.Items.FirstOrDefault(item => item.Id == removedItem.Id);
                inventoryGridData.Items.Remove(removedItemData);
                inventoryGridData.Positions.Remove(removedPosition);
            });
        }

        public bool TryAddItemToGrid(Item item)
        {
            if (this is InventoryGridWithSubGrid subGrid)
            {
                foreach (var grid in subGrid.SubGrids)
                {
                    if (grid.TryAddItemToGrid(item))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                var position = FindFreePosition(item);
                if (position.HasValue)
                {
                    PlaceItem(item, position.Value, item.IsRotated.CurrentValue);
                    ItemsPositionsMap[item] = position.Value;
                    return true;
                }

                return false;
            }
            
        }

        private Vector2Int? FindFreePosition(Item item)
        {
            for (int y = 0; y < Height - item.Height.CurrentValue + 1; y++)
            {
                for (int x = 0; x < Width - item.Width.CurrentValue + 1; x++)
                {
                    if (CanPlaceItem(item, new Vector2Int(x, y), isRotated: false))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }
    
        private void PlaceItem(Item item, Vector2Int position, bool isRotated)
        {
            int itemWidth = isRotated ? item.Height.CurrentValue : item.Width.CurrentValue;
            int itemHeight = isRotated ? item.Width.CurrentValue : item.Height.CurrentValue;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    Grid.Value[i * Height + j] = true;
                }
            }
        }

        private bool CanPlaceItem(Item item, Vector2Int position, bool isRotated)
        {
            int itemWidth = isRotated ? item.Height.CurrentValue : item.Width.CurrentValue;
            int itemHeight = isRotated ? item.Width.CurrentValue : item.Height.CurrentValue;

            if (position.x < 0 || position.y < 0 || position.x + itemWidth > Width ||
                position.y + itemHeight > Height)
                return false;

            for (int i = position.x; i < position.x + itemWidth; i++)
            {
                for (int j = position.y; j < position.y + itemHeight; j++)
                {
                    if (Grid.Value[i * Height + j]) return false;
                }
            }

            return true;
        }
    }
}