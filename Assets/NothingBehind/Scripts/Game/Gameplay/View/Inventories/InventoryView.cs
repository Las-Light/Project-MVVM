using NothingBehind.Scripts.Utils;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private GameObject _gridPrefab;
        [SerializeField] private GameObject _subGridPrefab;
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private RectTransform _subGridContainer;
        public int OwnerId { get; set; }

        private InventoryViewModel _inventoryViewModel;


        public void Bind(InventoryViewModel viewModel)
        {
            _inventoryViewModel = viewModel;
            OwnerId = viewModel.OwnerId;
            foreach (var inventoryGridViewModel in viewModel.AllInventoryGrids)
            {
                if (inventoryGridViewModel.IsSubGrid)
                {
                    CreateInventorSubGridView(inventoryGridViewModel);
                }
                else if (inventoryGridViewModel.SubGrids.Count == 0)
                {
                    CreateInventorGridView(inventoryGridViewModel);
                }
            }

            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 2, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            _gridContainer.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
        }

        // Перемещение предмета по позиции внутри одной сетки
        public AddItemsToInventoryGridResult TryMoveItemInGrid(string gridTypeId, int itemId,
            Vector2Int position, int amount)
        {
            return _inventoryViewModel.TryMoveItemInGrid(gridTypeId, itemId, position, amount);
        }

        // Перемещение предмета по позиции в другую сетку внутри одного инвентарая 
        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo,
            int itemId, Vector2Int position, int amount)
        {
            return _inventoryViewModel.TryMoveItemToAnotherGrid(gridTypeIdAt, gridTypeIdTo,
                itemId, position, amount);
        }

        // Автоперемещение предмета из одной сетки в ругую
        public AddItemsToInventoryGridResult TryMoveItemToAnotherGrid(string gridTypeIdAt, string gridTypeIdTo,
            int itemId, int amount)
        {
            return _inventoryViewModel.TryMoveItemToAnotherGrid(gridTypeIdAt, gridTypeIdTo,
                itemId, amount);
        }

        // Перемещение предмета по позиции из одного инвентаря в другой
        public AddItemsToInventoryGridResult TryMoveItemToAnotherInventory(int ownerIdAt, int ownerIdTo,
            string gridTypeIdAt, string gridTypeIdTo, int itemId, Vector2Int position, int amount)
        {
            return _inventoryViewModel.TryMoveItemToAnotherInventory(ownerIdAt, ownerIdTo,
                gridTypeIdAt, gridTypeIdTo, itemId, position, amount);
        }

        // Автоперемещение предмета из одного инвентаря в другой
        public AddItemsToInventoryGridResult TryMoveItemToAnotherInventory(int ownerIdAt, int ownerIdTo,
            string gridTypeIdAt, string gridTypeIdTo, int itemId, int amount)
        {
            return _inventoryViewModel.TryMoveItemToAnotherInventory(ownerIdAt, ownerIdTo,
                gridTypeIdAt, gridTypeIdTo, itemId, amount);
        }

        // Попытаться удалить предмет из сетки
        public RemoveItemsFromInventoryGridResult TryRemoveItem(string gridTypeId, int itemId)
        {
            return _inventoryViewModel.TryRemoveItem(gridTypeId, itemId);
        }

        // Полпытаться удалить некоторое количество из стека предмета
        public RemoveItemsFromInventoryGridResult TryRemoveItem(string gridTypeId, int itemId, int amount)
        {
            return _inventoryViewModel.TryRemoveItem(gridTypeId, itemId, amount);
        }


        private void CreateInventorGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var itemView = Instantiate(_gridPrefab, _gridContainer);
            itemView.GetComponent<InventoryGridView>().Bind(inventoryGridViewModel);
        }

        private void CreateInventorSubGridView(InventoryGridViewModel inventoryGridViewModel)
        {
            var itemView = Instantiate(_subGridPrefab, _subGridContainer);
            itemView.GetComponent<InventoryGridView>().Bind(inventoryGridViewModel);
        }
    }
}