using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments
{
    public class EquipmentView: MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private RectTransform _slotsContainer;

        private EquipmentViewModel _viewModel;
        private List<ItemView> _itemViews;
        
        public int OwnerId { get; set; }
        public void Bind(EquipmentViewModel viewModel, List<ItemView> itemViews)
        {
            _viewModel = viewModel;
            _itemViews = itemViews;

            foreach (var kvp in _viewModel.SlotsMap)
            {
                CreateSlotView(kvp.Key, kvp.Value);
            }

            // _viewModel.SlotsMap.ObserveReplace().Subscribe(e=>UpdateSlot(e.NewValue));
            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 3, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
        }

        private EquipmentSlotView CreateSlotView(SlotType slotType, EquipmentSlot equipmentSlot)
        {
            var slot = Instantiate(_slotPrefab, _slotsContainer);
            slot.name = slotType.ToString();
            var slotView = slot.GetComponent<EquipmentSlotView>();
            slotView.Bind(equipmentSlot, _viewModel, _itemViews);
            return slotView;
        }
        
    }
}