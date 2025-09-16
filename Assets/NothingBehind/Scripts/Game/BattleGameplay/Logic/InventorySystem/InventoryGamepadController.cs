using LitMotion;
using LitMotion.Extensions;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI.Inventories;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.InventorySystem
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

        private IView _currentGridView;
        private IView _startGridView;
        private Vector2Int _currentSlotPos;
        private float _lastNavigationTime;
        private ItemView _itemAtCursor;
        private ItemView _selectedItem;
        private Vector2Int _selectedItemPos;
        private bool _isDragging;

        private readonly CompositeDisposable _disposables = new();

        private GridNavigationService _navigationService;

        private void Start()
        {
            _navigationService = new GridNavigationService(
                _inventoryUIView.PlayerInventoryView.InventoryGridViews,
                _inventoryUIView.LootInventoryView.InventoryGridViews,
                _inventoryUIView.PlayerEquipmentView,
                _inventoryUIView.LootEquipmentView,
                _navigationConfig
            );

            // Выделяем первый доступный слот при открытии инвентаря
            _currentGridView =
                _inventoryUIView.PlayerEquipmentView; // Начинаем с первой сетки (можно хранить текущую)
            _currentSlotPos = Vector2Int.zero;
            _selectionIndicator.SetParent(_currentGridView.GetContainer());
            UpdateSelection();

            //Подписываемся на UI Input
            _inventoryUIView.InputManager.IsSubmit.Subscribe(OnSelect).AddTo(_disposables);
            _inventoryUIView.InputManager.IsCancel.Subscribe(OnCancel).AddTo(_disposables);
        }

        private void LateUpdate()
        {
            HandleNavigationInput(_inventoryUIView.InputManager.Navigation.CurrentValue);
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

            if (_currentGridView is EquipmentView equipmentView)
            {
                newPosition = CalculateEquipSlotBoundary(equipmentView, newPosition, navDirection);
                newPosition = RecalculateSlotOffset(equipmentView, newPosition);
            }

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
            
            if (_currentGridView is InventoryGridView gridView)
            {
                newPosition = CalculateItemBoundary(gridView, newPosition, navDirection);
            }

            if (_currentGridView is EquipmentView equipmentView)
            {
                newPosition = CalculateEquipSlotBoundary(equipmentView, newPosition, navDirection);
                newPosition = RecalculateSlotOffset(equipmentView, newPosition);
            }

            if (TryToSwitchView(ref newPosition, navDirection)) return;

            // Локальное перемещение внутри текущей сетки
            MoveLocalCursor(newPosition);
            UpdateSelection();
        }

        private bool TryToSwitchView(ref Vector2Int newSlotPosition, NavigationDirection navDirection)
        {
            if (IsOutOfBounds(newSlotPosition))
            {
                // Пытаемся найти следующий экран
                var nextViewAndPos = _navigationService.GetNextView(navDirection,_currentSlotPos, _currentGridView);
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

        // Корректировка позиции слота с учетом его начальной позиции
        private Vector2Int RecalculateSlotOffset(EquipmentView equipmentView, Vector2Int newPosition)
        {
            var nextSlot = equipmentView.GetSlotAt(newPosition);
            if (nextSlot != null)
            {
                newPosition = equipmentView.GetSlotPosition(nextSlot);
                return newPosition;
            }

            return newPosition;
        }

        private Vector2Int CalculateItemBoundary(InventoryGridView grid, Vector2Int newPosition,
            NavigationDirection direction)
        {
            var item = grid.GetItemViewAtPosition(_currentSlotPos);
            if (item == null || _isDragging) return newPosition;

            var itemPos = grid.GetItemPosition(item.Id);
            if (!itemPos.HasValue) return newPosition;

            var (left, right, top, bottom) = GetBounds(itemPos.Value, item.Size);
            return BoundaryCalculator.Calculate(newPosition, direction, left, right, top, bottom);
        }

        private Vector2Int CalculateEquipSlotBoundary(EquipmentView equipmentView,
            Vector2Int newPosition, NavigationDirection direction)
        {
            var slotView = equipmentView.GetSlotAt(_currentSlotPos);
            if (slotView == null) return newPosition;
            
            var (left, right, top, bottom) = GetBounds(newPosition, slotView.Size);
            return BoundaryCalculator.CalculateEquipBoundary(newPosition, direction, left, right, top, bottom);
        }

        private (int left, int right, int top, int bottom) GetBounds(Vector2Int pos, Vector2Int size) =>
            (pos.x, pos.x + size.x - 1, pos.y, pos.y + size.y - 1);

        private Vector2Int ClampPosition(Vector2Int position, int width, int height)
        {
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, width - 1),
                Mathf.Clamp(position.y, 0, height - 1)
            );
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