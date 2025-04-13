using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.UI.Inventories
{
    public class InventoryUIView : PopupView<InventoryUIViewModel>
    {
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _equipmentPrefab;
        [SerializeField] private RectTransform _playerInventoryContainer;
        [SerializeField] private RectTransform _playerEquipmentContainer;
        [SerializeField] private RectTransform _lootInventoryContainer;

        private List<ItemView> _itemViews = new ();
        private int _openInventoryId; // Id открытого инвентаря (противника, ящика или инвентаря на земле)

        protected override void OnBind(InventoryUIViewModel viewModel)
        {
            base.OnBind(viewModel);
            if (viewModel.PlayerId == viewModel.TargetOwnerId)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId), _itemViews);
                CreateEquipmentView(viewModel.GetEquipmentViewModel(viewModel.PlayerId), _itemViews);
                CreateEmptyInventoryView();
            }
            else
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId), _itemViews);
                CreateEquipmentView(viewModel.GetEquipmentViewModel(viewModel.PlayerId), _itemViews);
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId), _itemViews);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel.GetExitInventoryRequest(_openInventoryId);
        }

        private void CreatePlayerInventoryView(InventoryViewModel inventoryViewModel,
            List<ItemView> itemViews)
        {
            var inventoryUI = Instantiate(_inventoryPrefab, _playerInventoryContainer);
            var inventoryView = inventoryUI.GetComponent<InventoryView>();
            inventoryView.Bind(inventoryViewModel, itemViews);
        }

        private void CreateEmptyInventoryView()
        {
            var storageId = ViewModel.CreateStorage().EntityId;
            var inventoryViewModel = ViewModel.GetInventoryViewModel(storageId);
            _openInventoryId = inventoryViewModel.OwnerId;
            var inventoryUI = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            var inventoryView = inventoryUI.GetComponent<InventoryView>();
            inventoryView.Bind(inventoryViewModel, _itemViews);
        }

        private void CreateLootInventoryView(InventoryViewModel inventoryViewModel,
            List<ItemView> itemViews)
        {
            _openInventoryId = inventoryViewModel.OwnerId;
            var inventoryUI = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            var inventoryView = inventoryUI.GetComponent<InventoryView>();
            inventoryView.Bind(inventoryViewModel, itemViews);
        }

        private void CreateEquipmentView(EquipmentViewModel equipmentViewModel, 
            List<ItemView> itemViews)
        {
            var equipmentView = Instantiate(_equipmentPrefab, _playerEquipmentContainer);
            equipmentView.GetComponent<EquipmentView>().Bind(equipmentViewModel, itemViews);
        }
    }
}