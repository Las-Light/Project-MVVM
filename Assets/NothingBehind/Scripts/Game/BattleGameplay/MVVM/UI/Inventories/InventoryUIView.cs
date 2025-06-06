using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Equipments;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI.Inventories
{
    public class InventoryUIView : PopupView<InventoryUIViewModel>
    {
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _equipmentPrefab;
        [SerializeField] private RectTransform _playerInventoryContainer;
        [SerializeField] private RectTransform _playerEquipmentContainer;
        [SerializeField] private RectTransform _lootEquipmentContainer;
        [SerializeField] private RectTransform _lootInventoryContainer;

        public InventoryView PlayerInventoryView;
        public EquipmentView PlayerEquipmentView;
        public InventoryView LootInventoryView;
        public EquipmentView LootEquipmentView;

        public InputManager InputManager;
        public List<ItemView> ItemViews => _itemViews;

        private List<ItemView> _itemViews = new ();
        private int _openInventoryId; // Id открытого инвентаря (противника, ящика или инвентаря на земле)

        protected override void OnBind(InventoryUIViewModel viewModel)
        {
            base.OnBind(viewModel);
            InputManager = viewModel.InputManager;
            if (viewModel.TargetType == EntityType.Player)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.OwnerId), _itemViews);
                CreatePlayerEquipmentView(viewModel.GetEquipmentViewModel(viewModel.OwnerId), _itemViews);
                CreateEmptyInventoryView();
            }

            if (viewModel.TargetType == EntityType.Storage)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.OwnerId), _itemViews);
                CreatePlayerEquipmentView(viewModel.GetEquipmentViewModel(viewModel.OwnerId), _itemViews);
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId), _itemViews);
            }
            if (viewModel.TargetType == EntityType.Character)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.OwnerId), _itemViews);
                CreatePlayerEquipmentView(viewModel.GetEquipmentViewModel(viewModel.OwnerId), _itemViews);
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId), _itemViews);
                CreateLootEquipmentView(viewModel.GetEquipmentViewModel(viewModel.TargetOwnerId), _itemViews);
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
            PlayerInventoryView = inventoryView;
        }

        private void CreateEmptyInventoryView()
        {
            var storageId = ViewModel.CreateStorage().EntityId;
            var inventoryViewModel = ViewModel.GetInventoryViewModel(storageId);
            _openInventoryId = inventoryViewModel.OwnerId;
            var inventoryUI = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            var inventoryView = inventoryUI.GetComponent<InventoryView>();
            inventoryView.Bind(inventoryViewModel, _itemViews);
            LootInventoryView = inventoryView;
        }

        private void CreateLootInventoryView(InventoryViewModel inventoryViewModel,
            List<ItemView> itemViews)
        {
            _openInventoryId = inventoryViewModel.OwnerId;
            var inventoryUI = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            var inventoryView = inventoryUI.GetComponent<InventoryView>();
            inventoryView.Bind(inventoryViewModel, itemViews);
            LootInventoryView = inventoryView;
        }

        private void CreatePlayerEquipmentView(EquipmentViewModel equipmentViewModel, 
            List<ItemView> itemViews)
        {
            var equipmentGO = Instantiate(_equipmentPrefab, _playerEquipmentContainer);
            var equipmentView = equipmentGO.GetComponent<EquipmentView>();
            equipmentView.Bind(equipmentViewModel, itemViews);
            PlayerEquipmentView = equipmentView;
        }
        
        private void CreateLootEquipmentView(EquipmentViewModel equipmentViewModel, 
            List<ItemView> itemViews)
        {
            var equipmentGO = Instantiate(_equipmentPrefab, _lootEquipmentContainer);
            var equipmentView = equipmentGO.GetComponent<EquipmentView>();
            equipmentView.Bind(equipmentViewModel, itemViews);
            LootEquipmentView = equipmentView;
        }
    }
}