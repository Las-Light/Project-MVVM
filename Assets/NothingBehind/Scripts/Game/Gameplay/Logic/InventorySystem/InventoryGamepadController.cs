using System.Collections.Generic;
using System.Linq;
using LitMotion;
using LitMotion.Extensions;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InventorySystem
{
    public class InventoryGamepadController : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private float _navigationDelay = 0.3f;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private InventoryUIView _inventoryUIView;
        [SerializeField] private Button _closeButton;

        [Header("References")] [SerializeField]
        private RectTransform _selectionIndicator;

        private ReactiveProperty<Vector2> anchoredPosition = new();

        private IView _currentGridView;
        private Vector2Int _currentSlotPos;
        private float _lastNavigationTime;
        private ItemView _itemAtCursor;
        private ItemView _selectedItem;
        private Vector2Int _selectedItemPos;
        private bool _isDragging;

        private readonly CompositeDisposable _disposables = new();
        private List<InventoryGridView> _playerInventoryViews;
        private List<InventoryGridView> _lootInventoryViews;
        private EquipmentView _equipmentView;

        private Vector2Int _previousSlot;

        private enum NavigationDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        private void Start()
        {
            _lootInventoryViews = _inventoryUIView.LootInventoryView.InventoryGridViews;
            _playerInventoryViews = _inventoryUIView.PlayerInventoryView.InventoryGridViews;
            _equipmentView = _inventoryUIView.PlayerEquipmentView;

            // Выделяем первый доступный слот при открытии инвентаря
            _currentGridView =
                _inventoryUIView.PlayerEquipmentView; // Начинаем с первой сетки (можно хранить текущую)
            _currentSlotPos = Vector2Int.zero;
            _selectionIndicator.SetParent(_currentGridView.GetContainer());
            anchoredPosition.Subscribe(value =>
            {
                _selectionIndicator.anchoredPosition = value;
            }).AddTo(_disposables);
            UpdateSelection();

            //Подписываемся на UI Input
            _inventoryUIView.GameplayInputManager.IsSubmit.Subscribe(OnSelect).AddTo(_disposables);
            _inventoryUIView.GameplayInputManager.IsCancel.Subscribe(OnCancel).AddTo(_disposables);
        }

        private void LateUpdate()
        {
            HandleNavigationInput(_inventoryUIView.GameplayInputManager.Navigation.CurrentValue);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void HandleNavigationInput(Vector2 input)
        {
            if (Time.time - _lastNavigationTime < _navigationDelay) return;

            if (input.magnitude > 0.1f)
            {
                Vector2Int direction = new Vector2Int(
                    Mathf.RoundToInt(input.x),
                    -Mathf.RoundToInt(input.y)
                );
                if (_isDragging)
                {
                    // При перетаскивании перемещаем предмет
                    MoveDraggedItem(direction);
                }
                else
                {
                    // В обычном режиме перемещаем курсор
                    //_selectionIndicator.gameObject.SetActive(true);
                    MoveCursor(direction);
                }

                _lastNavigationTime = Time.time;
            }
        }

        private void MoveDraggedItem(Vector2Int direction)
        {
            if (_selectedItem == null) return;

            // Определяем направление навигации
            var navDirection = GetNavigationDirection(direction);

            // Вычисляем новую позицию для предмета
            Vector2Int newPosition = _currentSlotPos + direction;

            if (TryToSwitchView(ref newPosition, navDirection)) return;

            // Обновляем позицию
            _currentSlotPos = ClampPosition(newPosition, _currentGridView.Width, _currentGridView.Height);

            // Перемещаем предмет визуально
            _selectedItem.DragGamepad(_currentSlotPos, _currentGridView);

            // Обновляем подсветку
            UpdateSelection();
        }

        private void MoveCursor(Vector2Int direction)
        {
            // Определяем направление навигации
            var navDirection = GetNavigationDirection(direction);

            var newPosition = _currentSlotPos + direction;
            var newSlotPosition = newPosition;
            if (_currentGridView is InventoryGridView)
            {
                newSlotPosition = CalculateItemBoundary(_currentGridView, newPosition, direction);
            }

            if (TryToSwitchView(ref newSlotPosition, navDirection)) return;

            // Локальное перемещение внутри текущей сетки
            MoveLocalCursor(newSlotPosition);
            UpdateSelection();
        }

        private bool TryToSwitchView(ref Vector2Int newSlotPosition, NavigationDirection navDirection)
        {
            if (IsOutOfBounds(newSlotPosition))
            {
                // Пытаемся найти следующий экран
                var nextView = GetNextView(navDirection);
                if (nextView != null)
                {
                    SwitchToView(nextView);
                    if (_isDragging)
                    {
                        _selectedItem.DragGamepad(_currentSlotPos, _currentGridView);
                    }

                    return true;
                }
            }

            return false;
        }

        private void UpdateSelection()
        {
            // Получаем предмет в текущем слоте
            var newSelectedItem = _currentGridView.GetItemViewAtPosition(_currentSlotPos);

            // Обновляем выделение
            if (_itemAtCursor != null && !_isDragging) _itemAtCursor.SetHighlight(false);

            _itemAtCursor = newSelectedItem;

            if (_itemAtCursor != null && !_isDragging) _itemAtCursor.SetHighlight(true);

            // Обновляем позицию индикатора выделения
            if (newSelectedItem != null && !_isDragging)
            {
                var itemPos = _currentGridView.GetItemPosition(newSelectedItem.Id);
                if (itemPos.HasValue)
                {
                    _currentSlotPos = _selectedItemPos = itemPos.Value;
                }

                switch (_currentGridView)
                {
                    case InventoryGridView inventoryGridView:
                        _selectionIndicator.anchoredPosition = new Vector2(
                            _currentSlotPos.x * inventoryGridView.CellSize,
                            -_currentSlotPos.y * inventoryGridView.CellSize
                        );
                        break;
                    case EquipmentView equipmentView:
                        _selectionIndicator.anchoredPosition = new Vector2(
                            _currentSlotPos.x * equipmentView.CellSize + equipmentView.Spacing * _currentSlotPos.x,
                            -_currentSlotPos.y * equipmentView.CellSize - equipmentView.Spacing * _currentSlotPos.y
                        );
                        break;
                }

                AnimationChangeSizeIndicator(newSelectedItem.RectTransform.sizeDelta);
            }
            else
            {
                var targetPosition = Vector2.zero;
                switch (_currentGridView)
                {
                    case InventoryGridView inventoryGridView:
                        targetPosition = new Vector2(
                            _currentSlotPos.x * inventoryGridView.CellSize,
                            -_currentSlotPos.y * inventoryGridView.CellSize
                        );
                        break;
                    case EquipmentView equipmentView:
                        targetPosition = new Vector2(
                            _currentSlotPos.x * equipmentView.CellSize + equipmentView.Spacing * _currentSlotPos.x,
                            -_currentSlotPos.y * equipmentView.CellSize - equipmentView.Spacing * _currentSlotPos.y
                        );
                        break;
                }

                AnimationTransitionAnchorPos(targetPosition);
                if (!_isDragging)
                {
                    AnimationChangeSizeIndicator(new Vector2(_currentGridView.CellSize, _currentGridView.CellSize));
                }
                else
                {
                    AnimationChangeSizeIndicator(_selectedItem.RectTransform.sizeDelta);
                }
            }
        }

        private void AnimationChangeSizeIndicator(Vector2 targetSize)
        {
            LMotion.Create(_selectionIndicator.sizeDelta, targetSize, 0.1f)
                .WithEase(Ease.OutElastic)
                .BindToSizeDelta(_selectionIndicator)
                .AddTo(gameObject);
        }

        private void AnimationTransitionAnchorPos(Vector2 targetPosition)
        {
            LMotion.Create(_selectionIndicator.anchoredPosition, targetPosition, 0.1f)
                .WithEase(Ease.OutBounce)
                .BindToAnchoredPosition(_selectionIndicator)
                .AddTo(gameObject);
        }

        private void OnSelect(bool pressed)
        {
            if (!pressed) return;

            if (!_isDragging && _itemAtCursor != null)
            {
                // Начинаем перетаскивание
                _isDragging = true;
                _selectedItem = _itemAtCursor;
                _selectedItem.StartDragGamepad();
                //_selectionIndicator.gameObject.SetActive(false);
            }
            else if (_isDragging)
            {
                // Пытаемся разместить предмет
                _selectedItem.EndDragGamepad(
                    _currentSlotPos,
                    _currentGridView,
                    false
                );

                _isDragging = false;
                //_selectionIndicator.gameObject.SetActive(true);
            }

            UpdateSelection();
        }

        private void OnCancel(bool pressed)
        {
            if (pressed && _isDragging)
            {
                // Отмена перетаскивания - возвращаем на исходную позицию
                _selectedItem.EndDragGamepad(
                    _currentSlotPos,
                    _currentGridView,
                    pressed
                );
                _isDragging = false;
                //_selectionIndicator.gameObject.SetActive(true);
                _currentSlotPos = _selectedItemPos;
                UpdateSelection();
            }
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (context.performed && _isDragging)
            {
                // _selectedItem.Rotate();
            }
        }

        private IView GetNextView(NavigationDirection direction)
        {
            switch (_currentGridView)
            {
                case InventoryGridView inventoryGridView:
                    InventoryGridView inventoryView = null;
                    if (_lootInventoryViews.Contains(inventoryGridView))
                    {
                        inventoryView = GetTargetInventorySection(_currentGridView, _lootInventoryViews, direction);
                    }

                    if (_playerInventoryViews.Contains(inventoryGridView))
                    {
                        inventoryView = GetTargetInventorySection(_currentGridView, _playerInventoryViews, direction);
                    }

                    if (inventoryView != null)
                    {
                        return inventoryView;
                    }

                    if (_lootInventoryViews.Contains(inventoryGridView))
                    {
                        if (direction is NavigationDirection.Left)
                            return _equipmentView;
                    }

                    if (_playerInventoryViews.Contains(inventoryGridView))
                    {
                        if (direction is NavigationDirection.Right)
                            return _equipmentView; // Переход к экипировке
                    }

                    break;

                case EquipmentView:
                    if (direction == NavigationDirection.Left)
                        return GetTargetInventorySection(_currentGridView, _playerInventoryViews, direction);
                    if (direction == NavigationDirection.Right)
                        return GetTargetInventorySection(_currentGridView, _lootInventoryViews, direction);
                    break;
            }

            return null;
        }

        private NavigationDirection GetNavigationDirection(Vector2Int input)
        {
            if (input.x > 0) return NavigationDirection.Right;
            if (input.x < 0) return NavigationDirection.Left;
            if (input.y > 0) return NavigationDirection.Down;
            return NavigationDirection.Up;
        }

        private bool IsOutOfBounds(Vector2Int newSlotPos)
        {
            var isOutOfBounds =
                newSlotPos.x < 0 || newSlotPos.x >= _currentGridView.Width ||
                newSlotPos.y < 0 || newSlotPos.y >= _currentGridView.Height;
            return isOutOfBounds;
        }

        private void SwitchToView(IView newView)
        {
            var nearestSlot = FindNearestSlotPosition(_currentGridView, _currentSlotPos, newView);
            // Устанавливаем новый экран
            _currentGridView = newView;

            // Вычисляем стартовую позицию
            _currentSlotPos = nearestSlot;

            // Обновляем визуал
            _selectionIndicator.SetParent(newView.GetContainer());
            UpdateSelection();
        }

        private void MoveLocalCursor(Vector2Int newPosition)
        {
            _currentSlotPos = ClampPosition(newPosition, _currentGridView.Width, _currentGridView.Height);
        }

        private Vector2Int CalculateItemBoundary(IView grid, Vector2Int newPosition, Vector2Int direction)
        {
            var itemAtCurrentSlot =
                grid.GetItemViewAtPosition(_currentSlotPos); // Получаем предмет в текущей позиции
            Vector2Int? itemPosition = null;
            if (itemAtCurrentSlot != null)
            {
                itemPosition = grid.GetItemPosition(itemAtCurrentSlot.Id);
            }

            // Если под курсором есть предмет (и не в режиме перетаскивания), учитываем его размер при перемещении
            if (itemPosition.HasValue && !_isDragging)
            {
                _currentSlotPos = itemPosition.Value;
                // Определяем границы предмета
                int itemLeft = itemPosition.Value.x;
                int itemRight = itemPosition.Value.x + itemAtCurrentSlot.Width - 1;
                int itemTop = itemPosition.Value.y;
                int itemBottom = itemPosition.Value.y + itemAtCurrentSlot.Height - 1;

                // Корректируем новую позицию в зависимости от направления
                if (direction.x > 0) // Движение вправо
                {
                    newPosition.x = itemRight + 1;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, itemTop, itemBottom);
                }
                else if (direction.x < 0) // Движение влево
                {
                    newPosition.x = itemLeft - 1;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, itemTop, itemBottom);
                }
                else if (direction.y > 0) // Движение вниз (в Unity y увеличивается вниз)
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, itemLeft, itemRight);
                    newPosition.y = itemBottom + 1;
                }
                else if (direction.y < 0) // Движение вверх
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, itemLeft, itemRight);
                    newPosition.y = itemTop - 1;
                }
            }

            return newPosition;
        }

        private Vector2Int ClampPosition(Vector2Int position, int width, int height)
        {
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, width - 1),
                Mathf.Clamp(position.y, 0, height - 1)
            );
        }

        private InventoryGridView GetTargetInventorySection(IView currentView,
            List<InventoryGridView> allInventoryViews,
            NavigationDirection direction)
        {
            var orderByIndexSubGrids = OrderByIndexSubGrids(allInventoryViews);

            //Если переходим из экипировки
            if (currentView is EquipmentView)
            {
                var targetView = allInventoryViews.FirstOrDefault(view => view.GridType == InventoryGridType.Backpack);
                if (targetView != null)
                {
                    return targetView;
                }

                targetView = allInventoryViews.FirstOrDefault(view => view.GridType == InventoryGridType.ChestRig);
                if (targetView != null)
                {
                    return orderByIndexSubGrids.Last();
                }
            }

            //Если переходим из инвентаря
            var currentGridView = GetInventoryGridView(currentView);
            if (currentGridView.GridType == InventoryGridType.ChestRig && direction == NavigationDirection.Down)
            {
                var targetView = allInventoryViews.FirstOrDefault(view => view.GridType == InventoryGridType.Backpack);
                if (targetView != null)
                {
                    return targetView;
                }

                return null;
            }

            if (currentGridView.GridType == InventoryGridType.ChestRig
                && currentGridView.GridIndex != orderByIndexSubGrids.Last().GridIndex
                && direction == NavigationDirection.Right)
            {
                var targetView =
                    allInventoryViews.FirstOrDefault(view => view.GridIndex == currentGridView.GridIndex + 1);
                if (targetView != null)
                {
                    return targetView;
                }

                return null;
            }

            if (currentGridView.GridType == InventoryGridType.ChestRig
                && currentGridView.GridIndex != orderByIndexSubGrids.First().GridIndex
                && direction == NavigationDirection.Left)
            {
                var targetView =
                    allInventoryViews.FirstOrDefault(view => view.GridIndex == currentGridView.GridIndex - 1);
                if (targetView != null)
                {
                    return targetView;
                }

                return null;
            }

            if (currentGridView.GridType == InventoryGridType.Backpack && direction == NavigationDirection.Up)
            {
                var targetView = allInventoryViews.FirstOrDefault(view => view.GridType == InventoryGridType.ChestRig);
                if (targetView != null)
                {
                    return GetInventorySubGrid(allInventoryViews, InventoryGridType.ChestRig, _currentSlotPos);
                }

                return null;
            }

            return null;
        }

        private List<InventoryGridView> OrderByIndexSubGrids(List<InventoryGridView> allGridViews)
        {
            var orderByIndexSubGrids = allGridViews
                .Where(g => g.GridType == InventoryGridType.ChestRig)
                .OrderBy(g => g.GridIndex)
                .ToList();
            return orderByIndexSubGrids;
        }

        public Vector2Int FindNearestSlotPosition(IView sourceView,
            Vector2Int sourceSlotPos,
            IView targetView)
        {
            // Получаем мировую позицию исходного слота
            Vector2 sourceWorldPos = sourceView.GetSlotWorldPosition(sourceSlotPos);

            switch (targetView)
            {
                case InventoryGridView targetGrid:
                    // Для инвентарной сетки
                    return FindNearestInGrid(sourceWorldPos, targetGrid);
                case EquipmentView equipmentView:
                    // Для экрана экипировки
                    return FindNearestInEquipment(sourceWorldPos, equipmentView);
                default:
                    return Vector2Int.zero;
            }
        }

        private Vector2Int FindNearestInGrid(Vector2 sourceWorldPos,
            InventoryGridView targetGrid)
        {
            float minDistance = float.MaxValue;
            Vector2Int nearestSlot = Vector2Int.zero;

            // Перебираем все слоты в целевой сетке
            for (int y = 0; y < targetGrid.Height; y++)
            {
                for (int x = 0; x < targetGrid.Width; x++)
                {
                    Vector2Int slotPos = new Vector2Int(x, y);
                    Vector2 slotWorldPos = targetGrid.GetSlotWorldPosition(slotPos);

                    float distance = Vector2.Distance(sourceWorldPos, slotWorldPos);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSlot = slotPos;
                    }
                }
            }

            return nearestSlot;
        }

        private Vector2Int FindNearestInEquipment(Vector2 sourceWorldPos,
            EquipmentView equipmentView)
        {
            float minDistance = float.MaxValue;
            Vector2Int nearestSlot = Vector2Int.zero;

            // Получаем все слоты экипировки
            var allSlots = equipmentView.AllSlotViews;

            foreach (var slot in allSlots)
            {
                Vector2 slotWorldPos = slot.transform.position;
                float distance = Vector2.Distance(sourceWorldPos, slotWorldPos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = equipmentView.GetSlotPosition(slot);
                }
            }

            return nearestSlot;
        }

        private InventoryGridView GetInventoryGridView(IView view)
        {
            return view switch
            {
                InventoryGridView gridView => gridView,
                _ => null
            };
        }

        private Vector2Int GetGlobalPositionAtGrid(List<InventoryGridView> allGridViews, InventoryGridView grid,
            Vector2Int localPosition)
        {
            // Получаем все сетки того же типа, отсортированные по GridIndex
            var sameTypeGrids = allGridViews
                .Where(g => g.GridType == grid.GridType)
                .OrderBy(g => g.GridIndex)
                .ToList();

            int xOffset = 0;

            // Вычисляем смещение до текущей сетки
            foreach (var g in sameTypeGrids)
            {
                if (g == grid) break;

                // Для горизонтального расположения сеток
                xOffset += g.Width;
            }

            return new Vector2Int(
                localPosition.x + xOffset, localPosition.y
            );
        }

        private InventoryGridView GetInventorySubGrid(List<InventoryGridView> allGridViews, InventoryGridType gridType,
            Vector2Int globalPos)
        {
            var sameTypeGrids = allGridViews
                .Where(g => g.GridType == gridType)
                .OrderBy(g => g.GridIndex)
                .ToList();

            int accumulatedWidth = 0;

            foreach (var grid in sameTypeGrids)
            {
                // Для горизонтальной компоновки
                if (globalPos.x >= accumulatedWidth && globalPos.x < accumulatedWidth + grid.Width)
                {
                    return grid;
                }

                accumulatedWidth += grid.Width;
            }

            return sameTypeGrids.Last();
        }
    }
}