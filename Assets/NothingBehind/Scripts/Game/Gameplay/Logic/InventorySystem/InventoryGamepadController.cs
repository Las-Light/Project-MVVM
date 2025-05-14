using System.Collections.Generic;
using System.Linq;
using LitMotion;
using LitMotion.Extensions;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Items;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.Inventories;
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
        [SerializeField] private GridNavigationConfig _navigationConfig;

        [Header("References")] [SerializeField]
        private RectTransform _selectionIndicator;

        private ReactiveProperty<Vector2> anchoredPosition = new();

        private IView _currentGridView;
        private IView _startGridView;
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
        private Dictionary<InventoryGridView, Vector2[]> _gridWorldPositionsCache = new();

        private Vector2Int _previousSlot;

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
            anchoredPosition.Subscribe(value => { _selectionIndicator.anchoredPosition = value; }).AddTo(_disposables);
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
                newSlotPosition = CalculateItemBoundary(_currentGridView, newPosition, navDirection);
            }

            if (_currentGridView is EquipmentView equipmentView)
            {
                newSlotPosition = CalculateEquipSlotBoundary(equipmentView, newPosition, navDirection);
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
                var nextViewAndPos = GetNextView(navDirection);
                if (nextViewAndPos.newView != null)
                {
                    SwitchToView(nextViewAndPos.newView, nextViewAndPos.posAtNewView);
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
                        AnimationChangeSizeIndicator(new Vector2(_currentGridView.CellSize, _currentGridView.CellSize));
                        break;
                    case EquipmentView equipmentView:
                        targetPosition = new Vector2(
                            _currentSlotPos.x * equipmentView.CellSize + equipmentView.Spacing * _currentSlotPos.x,
                            -_currentSlotPos.y * equipmentView.CellSize - equipmentView.Spacing * _currentSlotPos.y
                        );
                        var slotView = equipmentView.GetSlotAt(_currentSlotPos);
                        AnimationChangeSizeIndicator(slotView.GetRectTransform().sizeDelta);
                        break;
                }

                AnimationTransitionAnchorPos(targetPosition);
                if (_isDragging)
                {
                    AnimationChangeSizeIndicator(_selectedItem.RectTransform.sizeDelta);
                }
            }
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
                _startGridView = _currentGridView;
            }
            else if (_isDragging)
            {
                // Пытаемся разместить предмет
                if (!_selectedItem.EndDragGamepad(
                        _currentSlotPos,
                        _currentGridView,
                        false
                    ))
                {
                    _selectionIndicator.SetParent(_startGridView.GetContainer());
                    _currentGridView = _startGridView;
                    _currentSlotPos = _selectedItemPos;
                }

                _isDragging = false;
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
                _selectionIndicator.SetParent(_startGridView.GetContainer());
                _currentGridView = _startGridView;
                _currentSlotPos = _selectedItemPos;
                _isDragging = false;
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

        private (IView newView, Vector2Int posAtNewView) GetNextView(NavigationDirection direction)
        {
            switch (_currentGridView)
            {
                case InventoryGridView inventoryGridView:
                    (IView, Vector2Int) nextViewAndPos = new();
                    if (_lootInventoryViews.Contains(inventoryGridView))
                    {
                        nextViewAndPos =
                            GetTargetInventorySection(inventoryGridView, _lootInventoryViews, direction);

                        if (nextViewAndPos.Item1 == null && direction is NavigationDirection.Left)
                        {
                            nextViewAndPos =
                                FindNearestInGridsCached(_currentGridView, _currentSlotPos, _playerInventoryViews);
                            
                            if (nextViewAndPos.Item1 == null)
                            {
                                nextViewAndPos = (_equipmentView,
                                    FindNearestInEquipment(_currentGridView, _currentSlotPos, _equipmentView));
                            }
                        }
                    }

                    if (_playerInventoryViews.Contains(inventoryGridView))
                    {
                        nextViewAndPos =
                            GetTargetInventorySection(inventoryGridView, _playerInventoryViews, direction);
                        if (nextViewAndPos.Item1 == null)
                        {
                            if (direction is NavigationDirection.Right)
                            {
                                nextViewAndPos =
                                    FindNearestInGridsCached(_currentGridView, _currentSlotPos, _lootInventoryViews);
                            }

                            if (direction is NavigationDirection.Left)
                            {
                                nextViewAndPos = (_equipmentView,
                                    FindNearestInEquipment(_currentGridView, _currentSlotPos, _equipmentView));
                            }
                        }
                    }

                    if (nextViewAndPos.Item1 != null)
                    {
                        return nextViewAndPos;
                    }

                    break;

                case EquipmentView:
                    if (direction == NavigationDirection.Right)
                    {
                        (IView, Vector2Int) nextInventoryViewAndPos = FindNearestInGridsCached(_currentGridView, _currentSlotPos, _playerInventoryViews);
                        if (nextInventoryViewAndPos.Item1 == null)
                        {
                            nextInventoryViewAndPos = FindNearestInGridsCached(_currentGridView, _currentSlotPos,
                                _lootInventoryViews);
                        }

                        return nextInventoryViewAndPos;
                    }

                    break;
            }

            return (null, Vector2Int.zero);
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

        private void SwitchToView(IView newView, Vector2Int newPosition)
        {
            // Устанавливаем новый экран
            _currentGridView = newView;

            // Вычисляем стартовую позицию
            _currentSlotPos = newPosition;

            // Обновляем визуал
            _selectionIndicator.SetParent(newView.GetContainer());
            UpdateSelection();
        }

        private void MoveLocalCursor(Vector2Int newPosition)
        {
            _currentSlotPos = ClampPosition(newPosition, _currentGridView.Width, _currentGridView.Height);
        }

        private Vector2Int CalculateItemBoundary(IView grid,
            Vector2Int newPosition, NavigationDirection direction)
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
                if (direction == NavigationDirection.Right) // Движение вправо
                {
                    newPosition.x = itemRight + 1;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, itemTop, itemBottom);
                }
                else if (direction == NavigationDirection.Left) // Движение влево
                {
                    newPosition.x = itemLeft - 1;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, itemTop, itemBottom);
                }
                else if (direction == NavigationDirection.Down) // Движение вниз (в Unity y увеличивается вниз)
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, itemLeft, itemRight);
                    newPosition.y = itemBottom + 1;
                }
                else if (direction == NavigationDirection.Up) // Движение вверх
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, itemLeft, itemRight);
                    newPosition.y = itemTop - 1;
                }
            }

            return newPosition;
        }

        private Vector2Int CalculateEquipSlotBoundary(EquipmentView equipmentView,
            Vector2Int newPosition, NavigationDirection direction)
        {
            var slotView = equipmentView.GetSlotAt(_currentSlotPos);
            if (slotView != null)
            {
                _currentSlotPos = equipmentView.GetSlotPosition(slotView);
                //Определяем границы слота
                int slotLeft = newPosition.x;
                int slotRight = newPosition.x + slotView.Width - 1;
                int slotTop = newPosition.y;
                int slotBottom = newPosition.y + slotView.Height - 1;

                // Корректируем новую позицию в зависимости от направления
                if (direction == NavigationDirection.Right)
                {
                    newPosition.x = slotRight;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, slotTop, slotBottom);
                }
                else if (direction == NavigationDirection.Left) // Движение влево
                {
                    newPosition.x = slotLeft;
                    newPosition.y = Mathf.Clamp(_currentSlotPos.y, slotTop, slotBottom);
                }
                else if (direction == NavigationDirection.Down) // Движение вниз (в Unity y увеличивается вниз)
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, slotLeft, slotRight);
                    newPosition.y = slotBottom;
                }
                else if (direction == NavigationDirection.Up) // Движение вверх
                {
                    newPosition.x = Mathf.Clamp(_currentSlotPos.x, slotLeft, slotRight);
                    newPosition.y = slotTop;
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

        private (IView newView, Vector2Int posAtNewView) GetTargetInventorySection(InventoryGridView currentGrid,
            List<InventoryGridView> allInventoryGridViews,
            NavigationDirection direction)
        {
            if (currentGrid == null) return (null, Vector2Int.zero);

            // Ищем подходящее правило перехода
            var transition = _navigationConfig.Transitions.FirstOrDefault(t =>
                t.SourceType == currentGrid.GridType && t.Direction == direction);

            if (transition == null) return (null, Vector2Int.zero);

            // Фильтруем сетки по целевому типу
            var targetGrids = allInventoryGridViews
                .Where(g => g.GridType == transition.TargetType)
                .OrderBy(g => g.GridIndex)
                .ToList();

            if (!targetGrids.Any()) return (null, Vector2Int.zero);

            // Если переход между сетками одного типа с изменением индекса
            if (!transition.IsTransToAnotherType && transition.SourceType == transition.TargetType)
            {
                int indexDelta = direction == NavigationDirection.Right ? 1 : -1;
                int targetIndex = currentGrid.GridIndex + indexDelta;

                // Проверяем границы допустимых индексов
                if (targetIndex >= 0 && targetIndex < targetGrids.Count)
                {
                    var sourceWorldPos = currentGrid.GetSlotWorldPosition(_currentSlotPos);
                    var targetPos = FindNearestInGrid(sourceWorldPos, targetGrids[targetIndex]);
                    return (targetGrids[targetIndex], targetPos);
                }

                return (null, Vector2Int.zero);
            }

            // Для вертикальных переходов между разными типами
            if (transition.IsTransToAnotherType)
            {
                return FindNearestInGridsCached(currentGrid, _currentSlotPos, targetGrids);
            }

            return (null, Vector2Int.zero);
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

        private Vector2Int FindNearestInEquipment(IView sourceView,
            Vector2Int sourceSlotPos,
            EquipmentView equipmentView)
        {
            float minDistance = float.MaxValue;
            Vector2Int nearestSlot = Vector2Int.zero;
            var sourceWorldPos = sourceView.GetSlotWorldPosition(sourceSlotPos);

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

        public (InventoryGridView grid, Vector2Int slotPos) FindNearestInGridsCached(IView sourceView,
            Vector2Int sourceSlotPos,
            List<InventoryGridView> targetGrids)
        {
            if (targetGrids == null || targetGrids.Count == 0)
                return (null, Vector2Int.zero);

            float minDistance = float.MaxValue;
            InventoryGridView nearestGrid = targetGrids[0];
            Vector2Int nearestSlot = Vector2Int.zero;
            var sourceWorldPos = sourceView.GetSlotWorldPosition(sourceSlotPos);

            foreach (var grid in targetGrids)
            {
                // Получаем или кэшируем мировые позиции слотов
                if (!_gridWorldPositionsCache.TryGetValue(grid, out var worldPositions))
                {
                    worldPositions = PrecalculateGridPositions(grid);
                    _gridWorldPositionsCache[grid] = worldPositions;
                }

                // Ищем ближайший слот в кэшированных позициях
                var (slotPos, distance) = FindNearestInCachedGrid(sourceWorldPos, grid, worldPositions);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestGrid = grid;
                    nearestSlot = slotPos;
                }
            }

            return (nearestGrid, nearestSlot);
        }

        private Vector2[] PrecalculateGridPositions(InventoryGridView grid)
        {
            Vector2[] positions = new Vector2[grid.Width * grid.Height];

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    positions[y * grid.Width + x] = grid.GetSlotWorldPosition(new Vector2Int(x, y));
                }
            }

            return positions;
        }

        private (Vector2Int slotPos, float distance) FindNearestInCachedGrid(Vector2 sourcePos, InventoryGridView grid,
            Vector2[] worldPositions)
        {
            int nearestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < worldPositions.Length; i++)
            {
                float dist = Vector2.Distance(sourcePos, worldPositions[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIndex = i;
                }
            }

            // Конвертируем индекс обратно в координаты
            int width = grid.Width;
            Vector2Int slotPos = new Vector2Int(nearestIndex % width, nearestIndex / width);

            return (slotPos, minDistance);
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
    }
}