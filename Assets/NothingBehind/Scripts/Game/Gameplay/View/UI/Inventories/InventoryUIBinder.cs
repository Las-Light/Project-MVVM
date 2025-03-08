using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.MVVM.UI;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.UI.Inventories
{
    public class InventoryUIBinder : PopupBinder<InventoryUIViewModel>
    {
        [SerializeField] private GameObject _inventoryPrefab;
        [SerializeField] private RectTransform _heroInventoryContainer;
        [SerializeField] private RectTransform _lootInventoryContainer;

        protected override void OnBind(InventoryUIViewModel viewModel)
        {
            base.OnBind(viewModel);
            if (viewModel.HeroId == viewModel.TargetOwnerId)
            {
                CreateHeroInventoryView(viewModel.GetInventoryViewModel(viewModel.HeroId));
            }
            else
            {
                CreateHeroInventoryView(viewModel.GetInventoryViewModel(viewModel.HeroId));
                CreateLootInventoryView(viewModel.GetInventoryViewModel(viewModel.TargetOwnerId));
            }
        }

        private void CreateHeroInventoryView(InventoryViewModel inventoryView)
        {
            var itemView = Instantiate(_inventoryPrefab, _heroInventoryContainer);
            itemView.GetComponent<InventoryView>().Bind(inventoryView);
        }

        private void CreateLootInventoryView(InventoryViewModel inventoryView)
        {
            var itemView = Instantiate(_inventoryPrefab, _lootInventoryContainer);
            itemView.GetComponent<InventoryView>().Bind(inventoryView);
        }
    }
}