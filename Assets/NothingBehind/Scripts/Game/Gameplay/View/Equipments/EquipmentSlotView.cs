using System;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.Gameplay.View.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.Equipments
{
    public class EquipmentSlotView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private float _cellSize;

        public SlotType SlotType;
        
        private EquipmentViewModel _viewModel;
        private ReadOnlyReactiveProperty<Item> _equippedItem;
        private IDisposable _disposable;
        private ItemView _itemView;
        private Image _slotImage;
        private Color _baseSlotColor;

        public void Bind(EquipmentSlot equipmentSlot, EquipmentViewModel viewModel)
        {
            SlotType = equipmentSlot.SlotType;
            _equippedItem = equipmentSlot.EquippedItem;
            _viewModel = viewModel;
            _slotImage = GetComponent<Image>();
            _baseSlotColor = _slotImage.color;
            
            _disposable = _equippedItem.Subscribe(item =>
            {
                if (item != null)
                {
                    UpdateVisual(item);
                }
                else
                {
                    if (_itemView != null)
                    {
                        Destroy(_itemView.gameObject);
                        _itemView = null;
                    }
                }
            });
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }

        public bool TryEquip(Item item)
        {
            if (item != null)
            {
                return _viewModel.TryEquipItem(SlotType, item);
            }

            return false;
        }

        public void Unequip()
        {
            _viewModel.TryUnequipItem(SlotType);
        }

        [CanBeNull]
        public Item GetItemAtSlot(SlotType slotType)
        {
            return _viewModel.GetItemAtSlot(slotType);
        }

        public bool CanEquipItem(SlotType slotType, Item item)
        {
            return _viewModel.CanEquipItem(slotType, item);
        }

        public void UpdateHighlight(SlotType slotType, Item item)
        {
            ClearHighlights();
            
            var canEquip = CanEquipItem(slotType, item);
            var itemAtSlot = _viewModel.GetItemAtSlot(SlotType);
            _slotImage.color = canEquip && itemAtSlot==null ? Color.green : Color.red;
        }

        public void ClearHighlights()
        {
            _slotImage.color = _baseSlotColor;
        }
        
        private void UpdateVisual(Item item)
        {
            var itemGameObject = Instantiate(_itemPrefab, transform);
            var itemView = itemGameObject.GetComponent<ItemView>();
            itemGameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            itemView.Bind(item, _cellSize);
            itemGameObject.transform.SetAsLastSibling();
            _itemView = itemView;
        }
    }
}