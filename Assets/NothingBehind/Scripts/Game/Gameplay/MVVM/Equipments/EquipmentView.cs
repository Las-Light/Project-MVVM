using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.State.Equipments;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments
{
    public class EquipmentView : MonoBehaviour, IView
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private RectTransform _slotsContainer;

        public int OwnerId { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public float CellSize => 100f;

        public float Spacing = 30f;

        public List<EquipmentSlotView> AllSlotViews => _allSlotViews;

        private readonly List<EquipmentSlotView> _allSlotViews = new();
        private readonly Dictionary<EquipmentSlotView, Vector2Int> _slotViewPositionMap = new();
        private EquipmentViewModel _viewModel;
        private List<ItemView> _itemViews;
        private bool[] _grid;

        public void Bind(EquipmentViewModel viewModel, List<ItemView> itemViews)
        {
            _viewModel = viewModel;
            _itemViews = itemViews;
            OwnerId = viewModel.OwnerId;
            Width = viewModel.Width;
            Height = viewModel.Height;
            _grid = new bool[Height * Width];

            // foreach (var kvp in _viewModel.SlotsMap)
            // {
            //     var slotView = CreateSlotView(kvp.Key, kvp.Value);
            //     _allSlotViews.Add(slotView);
            //     SetSlotPosition(kvp.Key, slotView);
            // }

            for (int i = 0; i < _viewModel.Slots.Count; i++)
            {
                var position = FindFreePosition(_viewModel.Slots[i]);
                if (position.HasValue)
                {
                    PlaceSlot(_viewModel.Slots[i], position.Value);
                    var slotView = CreateSlotView(_viewModel.Slots[i].SlotType, _viewModel.Slots[i]);
                    slotView.GetComponent<RectTransform>().anchoredPosition =
                        CalculateSlotAnchoredPosition(position.Value);
                    _allSlotViews.Add(slotView);
                    _slotViewPositionMap[slotView] = position.Value;
                }
            }

            // _viewModel.SlotsMap.ObserveReplace().Subscribe(e=>UpdateSlot(e.NewValue));
            // Задаем размер инвентаря в соответствии с размером экрана
            var viewScreenSize = new Vector2(Screen.width / 10*2, Screen.height);
            transform.parent.GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            GetComponent<RectTransform>().sizeDelta = viewScreenSize;
            //Spacing = _slotsContainer.GetComponent<VerticalLayoutGroup>().spacing;
            _slotsContainer.sizeDelta =
                new Vector2(Width * CellSize + Width * Spacing, Height * CellSize + Height * Spacing);
        }

        private Vector2 CalculateSlotAnchoredPosition(Vector2Int gridPosition)
        {
            float x = gridPosition.x * CellSize + Spacing * (gridPosition.x);
            float y = -gridPosition.y * CellSize - Spacing * (gridPosition.y);
            return new Vector2(x, y);
        }

        public EquipmentSlotView GetSlotAt(Vector2Int position)
        {
            foreach (var kvp in _slotViewPositionMap)
            {
                var slotView = kvp.Key;
                var slotPosition = kvp.Value;
                int slotWidth = slotView.Width;
                int slotHeight = slotView.Height;

                // Проверяем, находится ли позиция в пределах предмета
                if (position.x >= slotPosition.x && position.x < slotPosition.x + slotWidth &&
                    position.y >= slotPosition.y && position.y < slotPosition.y + slotHeight)
                {
                    return slotView;
                }
            }

            return null; // Позиция пуста
        }

        public Vector2Int GetSlotPosition(EquipmentSlotView slotView)
        {
            if (_slotViewPositionMap.TryGetValue(slotView, out var position))
            {
                return position;
            }

            return Vector2Int.zero;
        }

        public ItemView GetItemViewAtPosition(Vector2Int position)
        {
            var slotView = GetSlotAt(position);
            return slotView.GetItemViewAtSlot();
        }

        public RectTransform GetContainer()
        {
            return _slotsContainer;
        }

        public void ClearHighlights()
        {
            foreach (var slotView in _allSlotViews)
            {
                slotView.ClearHighlights();
            }
        }

        public EquipmentSlotView GetSlotViewHasItemView(ItemView itemView)
        {
            foreach (var slotView in _allSlotViews)
            {
                if (slotView.HasItemView(itemView))
                {
                    return slotView;
                }
            }

            return null;
        }

        public Vector2 GetSlotWorldPosition(Vector2Int slotPos)
        {
            var slot = GetSlotAt(slotPos);
            return slot?.transform.position ?? Vector2.zero;
        }

        public Vector2Int? GetItemPosition(int itemId)
        {
            var itemView = _itemViews.FirstOrDefault(view => view.Id == itemId);
            if (itemView == null) return null;
            var slotView = GetSlotViewHasItemView(itemView);
            if (slotView == null) return null;
            if (_slotViewPositionMap.TryGetValue(slotView, out var itemPosition))
            {
                return itemPosition;
            }

            return null;
        }

        private void PlaceSlot(EquipmentSlot slot, Vector2Int position)
        {
            for (int i = position.x; i < position.x + slot.Width; i++)
            {
                for (int j = position.y; j < position.y + slot.Height; j++)
                {
                    _grid[i * Height + j] = true;
                }
            }
        }

        private bool CanPlaceItem(EquipmentSlot slot, Vector2Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x + slot.Width > Width ||
                position.y + slot.Height > Height)
                return false;

            for (int i = position.x; i < position.x + slot.Width; i++)
            {
                for (int j = position.y; j < position.y + slot.Height; j++)
                {
                    if (_grid[i * Height + j]) return false;
                }
            }

            return true;
        }

        private Vector2Int? FindFreePosition(EquipmentSlot slot)
        {
            for (int y = 0; y < Height - slot.Height + 1; y++)
            {
                for (int x = 0; x < Width - slot.Width + 1; x++)
                {
                    if (CanPlaceItem(slot, new Vector2Int(x, y)))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null; // Нет свободного места
        }

        private void SetSlotPosition(SlotType slotType, EquipmentSlotView slotView)
        {
            switch (slotType)
            {
                case SlotType.ChestRig:
                    _slotViewPositionMap[slotView] = new Vector2Int(0, 0);
                    //slotView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 * CellSize, -0 * CellSize);
                    break;
                case SlotType.Backpack:
                    _slotViewPositionMap[slotView] = new Vector2Int(0, 1);
                    //slotView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 * CellSize, -1 * CellSize);
                    break;
                case SlotType.Weapon1:
                    _slotViewPositionMap[slotView] = new Vector2Int(0, 2);
                    //slotView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 * CellSize, -2 * CellSize);
                    break;
                case SlotType.Weapon2:
                    _slotViewPositionMap[slotView] = new Vector2Int(0, 3);
                    //slotView.GetComponent<RectTransform>().anchoredPosition = new Vector2(0 * CellSize, -3 * CellSize);
                    break;
            }
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