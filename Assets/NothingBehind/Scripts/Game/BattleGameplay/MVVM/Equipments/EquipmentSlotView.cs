using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Equipments
{
    public class EquipmentSlotView : MonoBehaviour
    {
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private float _cellSize;

        public SlotType SlotType;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2Int Size;

        private RectTransform _rectTransform;
        private EquipmentViewModel _viewModel;
        private ReadOnlyReactiveProperty<Item> _equippedItem;
        private IDisposable _disposable;
        private ItemView _itemView;
        private Image _slotImage;
        private Color _baseSlotColor;
        private List<ItemView> _itemViews;

        public void Bind(EquipmentSlot equipmentSlot, EquipmentViewModel viewModel,
            List<ItemView> itemViews)
        {
            SlotType = equipmentSlot.SlotType;
            _equippedItem = equipmentSlot.EquippedItem;
            _viewModel = viewModel;
            Width = equipmentSlot.Width;
            Height = equipmentSlot.Height;
            Size = new Vector2Int(Width, Height);
            _itemViews = itemViews;
            _slotImage = GetComponent<Image>();
            _baseSlotColor = _slotImage.color;
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(Width * _cellSize*2, Height * _cellSize*2);

            if (equipmentSlot.SlotType == SlotType.Backpack)
            {
                _cellSize /= 2;
            }

            if (equipmentSlot.SlotType is SlotType.Weapon1 or SlotType.Weapon2)
            {
                _cellSize /= 1.5f;
            }

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

        public RectTransform GetRectTransform()
        {
            return _rectTransform;
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
            _slotImage.color = canEquip && itemAtSlot == null ? Color.green : Color.red;
        }

        public void ClearHighlights()
        {
            _slotImage.color = _baseSlotColor;
        }

        public bool HasItemView(ItemView itemView)
        {
            return itemView == _itemView;
        }

        public ItemView GetItemViewAtSlot()
        {
            return _itemView;
        }

        private void UpdateVisual(Item item)
        {
            if (_viewModel.ItemViewModelsMap.TryGetValue(item.Id, out var viewModel))
            {
                Debug.Log("Update Visual");
                var itemGameObject = Instantiate(_itemPrefab, transform);
                itemGameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                var itemView = itemGameObject.GetComponent<ItemView>();
                _itemViews.Add(itemView);
                itemView.Bind(item, viewModel, _cellSize, _itemViews);
                itemGameObject.transform.SetAsLastSibling();
                _itemView = itemView;
            }
        }
    }
}