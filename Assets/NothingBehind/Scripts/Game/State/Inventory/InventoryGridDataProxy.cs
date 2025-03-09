using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    public class InventoryGridDataProxy
    {
        public int OwnerId { get; }
        public string GridTypeId { get; }
        public InventoryGridData Origin { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public bool IsSubGrid { get; }
        public List<InventoryGridDataProxy> SubGrids { get; } = new();
        public ReactiveProperty<bool[]> Grid { get; } // Одномерный массив для сериализации
        public ObservableList<ItemDataProxy> Items { get; } = new(); // Данные предметов
        public ObservableList<Vector2Int> Positions { get; } = new(); // Позиции предметов


        public InventoryGridDataProxy(InventoryGridData gridData)
        {
            Origin = gridData;
            OwnerId = gridData.OwnerId;
            GridTypeId = gridData.GridTypeId;
            Width = gridData.Width;
            Height = gridData.Height;
            CellSize = gridData.CellSize;
            IsSubGrid = gridData.IsSubGrid;
            Grid = new ReactiveProperty<bool[]>(gridData.Grid);
            gridData.SubGrids.ForEach(data => SubGrids.Add(new InventoryGridDataProxy(data)));
            gridData.Items.ForEach(data => Items.Add(new ItemDataProxy(data)));
            gridData.Positions.ForEach(Positions.Add);

            Grid.Subscribe(value => gridData.Grid = value);

            Items.ObserveAdd().Subscribe(e =>
            {
                var addedItemData = e.Value;
                gridData.Items.Add(addedItemData.Origin);
            });
            Positions.ObserveAdd().Subscribe(e =>
            {
                var addedPosition = e.Value;
                gridData.Positions.Add(addedPosition);
            });
            Items.ObserveRemove().Subscribe(e =>
            {
                var removedItemProxy = e.Value;
                var removedItem = gridData.Items.FirstOrDefault(item => item.Id == removedItemProxy.Id);
                gridData.Items.Remove(removedItem);
            });
            Positions.ObserveRemove().Subscribe(e =>
            {
                var removedPosition = e.Value;
                gridData.Positions.Remove(removedPosition);
            });
            Positions.ObserveReplace().Subscribe(e =>
            {
                var oldPosition = e.OldValue;
                var newValue = e.NewValue;
                var indexPosition = gridData.Positions.IndexOf(oldPosition);
                gridData.Positions[indexPosition] = newValue;
            });
        }
    }
}