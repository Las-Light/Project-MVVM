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
        public ReactiveProperty<bool[]> Grid { get; } // Одномерный массив для сериализации
        public ObservableList<Item> Items { get; } = new(); // Данные предметов
        public ObservableList<Vector2Int> Positions { get; } = new(); // Позиции предметов


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
            inventoryGridData.Items?.ForEach(data => Items.Add(ItemsFactory.CreateItem(data)));
            inventoryGridData.Positions?.ForEach(Positions.Add);

            Grid.Subscribe(value => inventoryGridData.Grid = value);

            Items.ObserveAdd().Subscribe(e =>
            {
                var addedItemData = e.Value;
                inventoryGridData.Items.Add(addedItemData.Origin);
            });
            Positions.ObserveAdd().Subscribe(e =>
            {
                var addedPosition = e.Value;
                inventoryGridData.Positions.Add(addedPosition);
            });
            Items.ObserveRemove().Subscribe(e =>
            {
                var removedItemProxy = e.Value;
                var removedItem = inventoryGridData.Items.FirstOrDefault(item => item.Id == removedItemProxy.Id);
                inventoryGridData.Items.Remove(removedItem);
            });
            Positions.ObserveRemove().Subscribe(e =>
            {
                var removedPosition = e.Value;
                inventoryGridData.Positions.Remove(removedPosition);
            });
            Positions.ObserveReplace().Subscribe(e =>
            {
                var oldPosition = e.OldValue;
                var newValue = e.NewValue;
                var indexPosition = inventoryGridData.Positions.IndexOf(oldPosition);
                inventoryGridData.Positions[indexPosition] = newValue;
            });
        }
    }
}