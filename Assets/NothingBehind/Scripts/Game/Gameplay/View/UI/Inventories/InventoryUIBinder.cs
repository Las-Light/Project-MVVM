using NothingBehind.Scripts.Game.Gameplay.View.Equipments;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI.Inventories
{
    public class InventoryUIBinder : PopupBinder<InventoryUIViewModel>
    {
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private GameObject _equipmentPrefab;
        [SerializeField] private RectTransform _playerInventoryContainer;
        [SerializeField] private RectTransform _playerEquipmentContainer;
        [SerializeField] private RectTransform _lootInventoryContainer;

        protected override void OnBind(InventoryUIViewModel viewModel)
        {
            base.OnBind(viewModel);
            if (viewModel.PlayerId == viewModel.TargetOwnerId)
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId));
                CreateEquipmentView(viewModel.GetEquipmentViewModel(viewModel.PlayerId));
            }
            else
            {
                CreatePlayerInventoryView(viewModel.GetInventoryViewModel(viewModel.PlayerId));
                CreateEquipmentView(viewModel.GetEquipmentViewModel(viewModel.PlayerId));
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId));
            }
        }

        private void CreatePlayerInventoryView(InventoryViewModel inventoryViewModel)
        {
            var inventoryView = Instantiate(_inventoryPrefab, _playerInventoryContainer);
            inventoryView.GetComponent<InventoryView>().Bind(inventoryViewModel);
        }

        private void CreateLootInventoryView(InventoryViewModel inventoryViewModel)
        {
            var inventoryView = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            inventoryView.GetComponent<InventoryView>().Bind(inventoryViewModel);
        }

        private void CreateEquipmentView(EquipmentViewModel equipmentViewModel)
        {
            var equipmentView = Instantiate(_equipmentPrefab, _playerEquipmentContainer);
            equipmentView.GetComponent<EquipmentView>().Bind(equipmentViewModel);
        }
    }
}