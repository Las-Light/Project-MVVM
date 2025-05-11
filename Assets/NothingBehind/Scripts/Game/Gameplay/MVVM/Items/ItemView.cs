using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.InventoryGridItems;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Items
{
    public class ItemView : MonoBehaviour, ISelectHandler, 
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text stackText;

        public int Id;
        public int Width;
        public int Height;
        public ReadOnlyReactiveProperty<bool> IsRotated;
        public RectTransform RectTransform;

        private Item _item;
        private ItemViewModel _itemViewModel;
        private float _cellSize;
        private ItemType _itemType;
        private bool _isHighlight;
        private bool _isStackable;
        private CanvasGroup _canvasGroup;
        private Image _itemImage;
        private Canvas _mainCanvas;
        private RectTransform _canvasRectTransform;

        private Vector2 _startPosition;
        private Transform _startParent;
        private IView _startView;
        private IView _currentView;

        private ReadOnlyReactiveProperty<int> _currentStack;
        private ReadOnlyReactiveProperty<int> _width;
        private ReadOnlyReactiveProperty<int> _height;

        private readonly CompositeDisposable _disposables = new();
        private List<ItemView> _itemViews;
        
        private InventoryGridView _currentGrid;

        public void Bind(Item item, ItemViewModel itemViewModel, float cellSize,
            List<ItemView> openedViews)
        {
            _item = item;
            _itemViewModel = itemViewModel;
            Width = itemViewModel.Width.Value;
            Height = itemViewModel.Height.Value;
            _itemType = item.ItemType;
            _currentStack = item.CurrentStack;
            _width = item.Width;
            _height = item.Height;
            IsRotated = item.IsRotated;
            Id = item.Id;
            _isStackable = item.IsStackable;
            _cellSize = cellSize;
            _itemViews = openedViews;

            RectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemImage = GetComponent<Image>();
            _itemImage.color = itemViewModel.ItemSettings.Color;
            _currentView = _startView = GetComponentInParent<IView>();
            _mainCanvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _mainCanvas.transform as RectTransform;

            _disposables.Add(_currentStack.Subscribe(_ => UpdateStack()));
            _disposables.Add(IsRotated.Subscribe(_ => UpdateRotate()));

            InitializeVisuals();
        }

        private void OnDestroy()
        {
            _itemViews.Remove(this);
            _disposables?.Dispose();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Запоминаем начальную позицию и родителя
            _startPosition = RectTransform.anchoredPosition;
            _startParent = RectTransform.parent;

            // 1. Визуальные изменения
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0.8f;
            // Отключаем блокировку raycast, чтобы предмет можно было перетаскивать
            _canvasGroup.blocksRaycasts = false;

            RectTransform.SetParent(_canvasRectTransform);

            // Перемещаем предмет на верхний слой
            RectTransform.SetAsLastSibling();
            
            LMotion.Create(transform.localScale, Vector3.one * 1.1f, 0.15f)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(transform)
                .AddTo(gameObject);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _currentView?.ClearHighlights();
            // Перемещаем предмет за курсором
            RectTransform.anchoredPosition += eventData.delta / _mainCanvas.scaleFactor;
            
            // Получаем целевую вьюху
            var targetViews = GetTargetInventoryView(eventData);

            // Обновляем подсветку
            if (targetViews is InventoryGridView gridView)
            {
                _currentView = gridView;
                var gridPos = CalculateGridPosition(eventData.position, gridView);
                gridView.UpdateHighlights(_item, gridPos);
            }

            if (targetViews is EquipmentView equipmentView)
            {
                _currentView = equipmentView;
                var equipPos = CalculateGridPosition(eventData.position, equipmentView);
                var slotView = equipmentView.GetSlotAt(equipPos);
                if (slotView != null)
                {
                    slotView.UpdateHighlight(slotView.SlotType, _item);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Включаем блокировку raycast обратно
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }

            // Получаем целевой инвентарь (через Raycast)
            var targetViews = GetTargetInventoryView(eventData);

            _currentView?.ClearHighlights();

            if (targetViews is InventoryGridView targetGridView)
            {
                _currentView = targetGridView;

                // Преобразуем позицию курсора в координаты сетки целевого инвентаря
                var gridPos = CalculateGridPosition(eventData.position, targetGridView);

                if (_startView is InventoryGridView gridView)
                {
                    //пытаемся положить предмет в предмет-сетку
                    if (TryPlaceToItem(targetGridView, gridPos, gridView))
                    {
                        return;
                    }

                    TryPlaceToGridAtGrid(gridView, targetGridView, gridPos);
                    return;
                }

                if (_startView is EquipmentView startEquipView)
                {
                    var slotView = startEquipView.GetSlotViewHasItemView(this);
                    if (slotView != null)
                    {
                        TryPlaceToGridAtEquipmentSlot(targetGridView, gridPos, slotView);
                    }
                    return;
                }
            }

            if (targetViews is EquipmentView equipmentView)
            {
                _currentView = equipmentView;
                
                var equipPos = CalculateGridPosition(eventData.position, equipmentView);
                var slotView = equipmentView.GetSlotAt(equipPos);

                var itemAtSlot = slotView.GetItemAtSlot(slotView.SlotType);
                if (_startView is InventoryGridView startGridView)
                {
                    TryEquipAtGrid(itemAtSlot, slotView, startGridView);
                    return;
                }

                if (_startView is EquipmentView startView)
                {
                    if (!TryEquipAtEquipmentSlot(itemAtSlot, slotView))
                    {
                        ReturnToStartPosition();
                    }

                    return;
                }
            }

            if (targetViews == null)
            {
                ReturnToStartPosition();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var itemView in _itemViews)
            {
                itemView.HighlightItem(_itemType);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var itemView in _itemViews)
            {
                itemView.ClearHighlight();
            }
        }

        public void SetHighlight(bool highlight)
        {
            // Реализация подсветки
            if (highlight)
            {
                var itemImageColor = _itemImage.color;
                itemImageColor.a = 0.8f;
                _itemImage.color = itemImageColor;
            }
            else
            {
                _itemImage.color = _itemViewModel.ItemSettings.Color;
            }
        }

        public void StartDragGamepad()
        {
            // Сохранение исходных данных
            _startPosition = RectTransform.anchoredPosition;
            _startParent = RectTransform.parent;
            
            // Визуальные изменения
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0.8f;
            // Отключаем блокировку raycast, чтобы предмет можно было перетаскивать
            _canvasGroup.blocksRaycasts = false;

            // Анимация поднятия
            RectTransform.SetAsLastSibling();
            LMotion.Create(transform.localScale, Vector3.one * 1.1f, 0.15f)
                .WithEase(Ease.OutBack)
                .BindToLocalScale(transform)
                .AddTo(gameObject);
        }

        public void DragGamepad(Vector2Int slotPosition, IView targetGrid)
        {
            _currentView?.ClearHighlights();
            RectTransform.SetParent(targetGrid.GetContainer());
            _cellSize = targetGrid.CellSize;
            // 2. Рассчитываем целевую позицию в мировых координатах
            Vector2 targetPosition = new Vector2(
                slotPosition.x * _cellSize,
                -slotPosition.y * _cellSize);
    
            LMotion.Create(RectTransform.anchoredPosition, 
                    targetPosition, 
                    0.1f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPosition(RectTransform)
                .AddTo(gameObject);

            // Обновляем подсветку
            if (targetGrid is InventoryGridView gridView)
            {
                _currentView = gridView;
                gridView.UpdateHighlights(_item, slotPosition);
            }

            if (targetGrid is EquipmentView equipmentView)
            {
                _currentView = equipmentView;
                var slotView = equipmentView.GetSlotAt(slotPosition);
                if (slotView != null)
                {
                    slotView.UpdateHighlight(slotView.SlotType, _item);
                }
            }
    
        }

        public void EndDragGamepad(Vector2Int slotPosition,
            IView targetView, bool isCancellation)
        {
            // 1. Восстановление визуальных параметров
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }
            
            _currentView?.ClearHighlights();
            _currentView = targetView;

            if (!isCancellation)
            {
                if (targetView is InventoryGridView targetGridView)
                {
                    var targetPosition = new Vector2(
                        slotPosition.x * _cellSize,
                        -slotPosition.y * _cellSize);
                    if (_startView is InventoryGridView startGridView)
                    {
                        //пытаемся положить предмет в предмет-сетку
                        if (TryPlaceToItem(targetGridView, slotPosition, startGridView))
                        {
                            //MovementAnimation(targetPosition);
                            return;
                        }
                        TryPlaceToGridAtGrid(startGridView, targetGridView, slotPosition);
                        //MovementAnimation(targetPosition);
                        return;
                    }

                    if (_startView is EquipmentView startEquipmentView)
                    {
                        var equipmentSlot = startEquipmentView.GetSlotViewHasItemView(this);
                        if (equipmentSlot != null)
                        {
                            TryPlaceToGridAtEquipmentSlot(targetGridView, slotPosition, equipmentSlot);
                        }
                        //MovementAnimation(targetPosition);
                        return;
                    }
                }

                if (targetView is EquipmentView targetEquipmentView)
                {
                    var slotView = targetEquipmentView.GetSlotAt(slotPosition);
                    var targetPosition = Vector2.zero;

                    var itemAtSlot = slotView.GetItemAtSlot(slotView.SlotType);
                    if (_startView is InventoryGridView startGridView)
                    {
                        TryEquipAtGrid(itemAtSlot, slotView, startGridView);
                        //MovementAnimation(targetPosition);
                        return;
                    }

                    if (_startView is EquipmentView)
                    {
                        if (TryEquipAtEquipmentSlot(itemAtSlot, slotView))
                        {
                            // MovementAnimation(targetPosition);
                        }
                        else
                        {
                            ReturnToStartPosition();
                        }
                    }
                }
            }
            else
            {
                // Возврат с bounce-эффектом
                ReturnToStartPosition();
            }
        }

        private void MovementAnimation(Vector2 targetPosition)
        {
            // Анимация перемещения 
            Debug.Log("targetPos - " + targetPosition);
            LMotion.Create(RectTransform.anchoredPosition, targetPosition, 0.2f)
                .WithEase(Ease.OutBack)
                .WithOnComplete(() =>
                {
                    // Анимация масштаба с проверкой существования объекта
                    if (this != null && transform != null)
                    {
                        LMotion.Create(transform.localScale, Vector3.one, 0.1f)
                            .BindToLocalScale(transform)
                            .AddTo(gameObject);
                    }
                })
                .BindToAnchoredPosition(RectTransform)
                .AddTo(gameObject);
        }

        private void HighlightItem(ItemType itemType)
        {
            if (itemType == _itemType && !_isHighlight)
            {
                _itemImage.color = Color.blue;
                _isHighlight = true;
            }
        }

        private void ClearHighlight()
        {
            _itemImage.color = _itemViewModel.ItemSettings.Color;
            _isHighlight = false;
        }

        private bool TryEquipAtEquipmentSlot(Item itemAtSlot, EquipmentSlotView slotView)
        {
            if (itemAtSlot != null)
            {
                return false;
            }
            return slotView.TryEquip(_item);
        }

        private void TryEquipAtGrid(Item itemAtSlot, EquipmentSlotView slotView, InventoryGridView startGridView)
        {
            if (itemAtSlot == null && slotView.CanEquipItem(slotView.SlotType, _item))
            {
                startGridView.RemoveItem(Id);
                slotView.TryEquip(_item);
                return;
            }

            ReturnToStartPosition();
        }

        private void TryPlaceToGridAtEquipmentSlot(InventoryGridView targetGridView, Vector2Int gridPos,
            EquipmentSlotView startSlotView)
        {
            var addedItemResult = targetGridView.AddItems(_item, gridPos, _currentStack.CurrentValue);

            if (addedItemResult.Success)
            {
                startSlotView.Unequip();
            }
            else
            {
                ReturnToStartPosition();
            }
        }

        private void TryPlaceToGridAtGrid(InventoryGridView atGridView, InventoryGridView targetGridView,
            Vector2Int gridPos)
        {
            var oldPosition = atGridView.GetItemPosition(Id);

            var removeItemResult = atGridView.RemoveItem(Id);
            var addedItemResult = targetGridView.AddItems(_item, gridPos, removeItemResult.ItemsToRemoveAmount);

            if (addedItemResult.Success)
            {
                if (!addedItemResult.NeedRemove)
                {
                    if (oldPosition != null)
                    {
                        atGridView.AddItems(_item, oldPosition.Value,
                            addedItemResult.ItemsNotAddedAmount);
                    }
                    else
                    {
                        atGridView.AddItems(_item, addedItemResult.ItemsNotAddedAmount);
                    }

                    RectTransform.SetParent(targetGridView.GridContainer.transform);
                    // Устанавливаем позицию предмета на основе координат ячейки сетки
                    Vector2 cellPosition = new Vector2(
                        gridPos.x * targetGridView.CellSize,
                        -gridPos.y * targetGridView.CellSize
                    );
                    RectTransform.anchoredPosition = cellPosition;
                    RectTransform.SetAsLastSibling();
                    _startView = targetGridView;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (oldPosition != null)
                {
                    atGridView.AddItems(_item, oldPosition.Value,
                        addedItemResult.ItemsNotAddedAmount);
                }
                else
                {
                    atGridView.AddItems(_item, addedItemResult.ItemsNotAddedAmount);
                }

                ReturnToStartPosition();
            }
        }

        private bool TryPlaceToItem(InventoryGridView targetGridView, Vector2Int gridPos,
            InventoryGridView atGridView)
        {
            var itemAtPosition = targetGridView.GetItemAtPosition(gridPos);
            if (itemAtPosition != null && itemAtPosition != _item)
            {
                if (itemAtPosition is GridItem gridItem)
                {
                    if (gridItem.Grid.Value.TryAddItemToGrid(_item))
                    {
                        atGridView.RemoveItem(Id);
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReturnToStartPosition()
        {
            // Если передача не удалась, возвращаем предмет на исходную позицию
            // _rectTransform.SetParent(_startParent);
            // _rectTransform.anchoredPosition = _startPosition;
            // Возврат с bounce-эффектом
            Debug.Log(_startPosition);
            
            RectTransform.SetParent(_startParent);
            LMotion.Create(RectTransform.anchoredPosition, _startPosition, 0.05f)
                .WithEase(Ease.OutBounce)
                .WithOnComplete(() =>
                {
                    if (this != null && RectTransform != null)
                    {
                        LMotion.Create(RectTransform.localScale, Vector3.one, 0.05f)
                            .BindToLocalScale(RectTransform)
                            .AddTo(gameObject);
                    }
                })
                .BindToAnchoredPosition(RectTransform)
                .AddTo(gameObject);
        }

        private void InitializeVisuals()
        {
            // Устанавливаем размер предмета в соответствии с его шириной и высотой
            RectTransform.sizeDelta = new Vector2(
                _width.CurrentValue * _cellSize,
                _height.CurrentValue * _cellSize
            );

            // Устанавливаем иконку предмета (предположим, что она хранится в ItemModel)
            //TODO: сделать подгрузку картинки из Reactive ItemDataProxy 
            //_itemImage.sprite = _item.Icon;

            // Если предмет стакаемый, отображаем количество
            if (_isStackable)
            {
                if (stackText != null)
                {
                    stackText.text = _currentStack.CurrentValue.ToString();
                }
            }
        }

        private void UpdateIcon()
        {
            //TODO: сделать подгрузку картинки из Reactive ItemDataProxy
            //_itemImage.sprite = _item.Icon;
        }

        private void UpdateRotate()
        {
            RectTransform.sizeDelta = new Vector2(
                _item.Width.Value * _cellSize,
                _item.Height.Value * _cellSize
            );
        }

        private void UpdateStack()
        {
            if (stackText != null)
            {
                stackText.text = _currentStack.CurrentValue.ToString();
            }
        }

        private Vector2Int CalculateGridPosition(Vector2 screenPosition, IView gridView)
        {
            // Получаем RectTransform сетки инвентаря
            RectTransform gridRectTransform = gridView.GetContainer();

            // Преобразуем экранные координаты в локальные координаты сетки
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRectTransform,
                screenPosition,
                null, // Камера (null для Canvas в режиме Screen Space - Overlay)
                out Vector2 localPoint
            );

            // Переводим локальные координаты в координаты сетки
            Vector2 normalizedPosition = Rect.PointToNormalized(gridRectTransform.rect, localPoint);

            // Размеры сетки
            int gridWidth = gridView.Width;
            int gridHeight = gridView.Height;

            // Вычисляем координаты ячейки
            int gridX = Mathf.FloorToInt(normalizedPosition.x * gridWidth);
            int gridY = Mathf.FloorToInt((1 - normalizedPosition.y) *
                                         gridHeight); // Инвертируем Y, так как (0,0) в UI — это верхний левый угол

            return new Vector2Int(gridX, gridY);
        }

        private IView GetTargetInventoryView(PointerEventData eventData)
        {
            IView targetView = null;

            GraphicRaycaster ray = GetComponentInParent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            ray.Raycast(eventData, results);

            // var results = eventData.hovered;
            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent(out InventoryGridView gridView))
                {
                    targetView = gridView;
                }

                if (result.gameObject.TryGetComponent(out EquipmentView slotView))
                {
                    targetView = slotView;
                }
            }

            return targetView;
        }

        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log("Select");
        }
    }
}