using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.View.Equipments;
using NothingBehind.Scripts.Game.Gameplay.View.Inventories;
using NothingBehind.Scripts.Game.State.Items;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.Items
{
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text stackText;

        public Item Item;

        private float _cellSize;
        private int _id;
        private bool _isStackable;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Image _itemImage;
        private Canvas _mainCanvas;
        private RectTransform _canvasRectTransform;

        private Vector2 _startPosition;
        private Transform _startParent;
        private IView _startView;
        private IView _currentView;

        private ReadOnlyReactiveProperty<int> _currentStack;
        private ReadOnlyReactiveProperty<bool> _isRotated;
        private ReadOnlyReactiveProperty<int> _width;
        private ReadOnlyReactiveProperty<int> _height;

        private readonly CompositeDisposable _disposables = new();

        public void Bind(Item item, float cellSize)
        {
            Item = item;
            _currentStack = item.CurrentStack;
            _width = item.Width;
            _height = item.Height;
            _isRotated = item.IsRotated;
            _id = item.Id;
            _isStackable = item.IsStackable;
            _cellSize = cellSize;

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemImage = GetComponent<Image>();
            _currentView = _startView = GetComponentInParent<IView>();
            _mainCanvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _mainCanvas.transform as RectTransform;

            _disposables.Add(_currentStack.Subscribe(_ => UpdateStack()));
            _disposables.Add(_isRotated.Subscribe(_ => UpdateRotate()));

            InitializeVisuals();
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Запоминаем начальную позицию и родителя
            _startPosition = _rectTransform.anchoredPosition;
            _startParent = _rectTransform.parent;

            // Отключаем блокировку raycast, чтобы предмет можно было перетаскивать
            _canvasGroup.blocksRaycasts = false;

            _rectTransform.SetParent(_canvasRectTransform);

            // Перемещаем предмет на верхний слой
            _rectTransform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _currentView?.ClearHighlights();
            // Перемещаем предмет за курсором
            _rectTransform.anchoredPosition += eventData.delta / _mainCanvas.scaleFactor;

            // Получаем целевую вьюху
            var targetViews = GetTargetInventoryView(eventData);

            // Обновляем подсветку
            if (targetViews is InventoryGridView gridView)
            {
                _currentView = gridView;
                var gridPos = CalculateGridPosition(eventData.position, gridView);
                gridView.UpdateHighlights(Item, gridPos);
            }

            if (targetViews is EquipmentSlotView slotView)
            {
                _currentView = slotView;
                slotView.UpdateHighlight(slotView.SlotType, Item);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Включаем блокировку raycast обратно
            _canvasGroup.blocksRaycasts = true;

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
                    var oldPosition = gridView.GetItemPosition(_id);

                    var removeItemResult = gridView.RemoveItem(_id);
                    var addedItemResult = targetGridView.AddItems(Item, gridPos, removeItemResult.ItemsToRemoveAmount);

                    if (addedItemResult.Success)
                    {
                        if (!addedItemResult.NeedRemove)
                        {
                            if (oldPosition != null)
                            {
                                gridView.AddItems(Item, oldPosition.Value,
                                    addedItemResult.ItemsNotAddedAmount);
                            }
                            else
                            {
                                gridView.AddItems(Item, addedItemResult.ItemsNotAddedAmount);
                            }

                            _rectTransform.SetParent(targetGridView.GridContainer.transform);
                            // Устанавливаем позицию предмета на основе координат ячейки сетки
                            Vector2 cellPosition = new Vector2(
                                gridPos.x * targetGridView.CellSize,
                                -gridPos.y * targetGridView.CellSize
                            );
                            _rectTransform.anchoredPosition = cellPosition;
                            _rectTransform.SetAsLastSibling();
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
                            gridView.AddItems(Item, oldPosition.Value,
                                addedItemResult.ItemsNotAddedAmount);
                        }
                        else
                        {
                            gridView.AddItems(Item, addedItemResult.ItemsNotAddedAmount);
                        }

                        ReturnToStartPosition();
                    }

                    return;
                }

                if (_startView is EquipmentSlotView startSlotView)
                {
                    var addedItemResult = targetGridView.AddItems(Item, gridPos, _currentStack.CurrentValue);

                    if (addedItemResult.Success)
                    {
                        startSlotView.Unequip();
                    }
                    else
                    {
                        ReturnToStartPosition();
                    }

                    return;
                }
            }

            if (targetViews is EquipmentSlotView slotView)
            {
                _currentView = slotView;
                
                var itemAtSlot = slotView.GetItemAtSlot(slotView.SlotType);
                if (_startView is InventoryGridView startGridView)
                {
                    //TODO: Если из сетки где лежит предмет-сетка(рюкзак например) экипировать этот предмет,
                    //TODO: то куда деть предмет который был экипирован до этого
                    // if (itemAtSlot != null && slotView.CanEquipItem(slotView.SlotType, Item))
                    // {
                    //     slotView.Unequip();
                    //     if (slotView.TryEquip(Item))
                    //     {
                    //         var itemPosition = startGridView.GetItemPosition(_id);
                    //         if (itemPosition != null)
                    //         {
                    //             startGridView.RemoveItem(_id);
                    //             startGridView.AddItems(itemAtSlot, itemPosition.Value,
                    //                 itemAtSlot.CurrentStack.CurrentValue);
                    //             _startView = slotView;
                    //             return;
                    //         }
                    //     }
                    // }

                    if (itemAtSlot == null && slotView.CanEquipItem(slotView.SlotType, Item))
                    {
                        startGridView.RemoveItem(_id);
                        slotView.TryEquip(Item);
                        return;
                    }
                    ReturnToStartPosition();
                    return;
                }

                if (_startView is EquipmentSlotView startView)
                {
                    //TODO: здесь может быть ситуация когда экипированный предмет врага сразу экипируется на игрока
                    //TODO: куда положить предмет который был экипирован???
                    if (slotView.TryEquip(Item))
                    {
                        startView.Unequip();
                    }
                    else
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

        private void ReturnToStartPosition()
        {
            // Если передача не удалась, возвращаем предмет на исходную позицию
            _rectTransform.SetParent(_startParent);
            _rectTransform.anchoredPosition = _startPosition;
        }

        private void InitializeVisuals()
        {
            // Устанавливаем размер предмета в соответствии с его шириной и высотой
            _rectTransform.sizeDelta = new Vector2(
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
            _rectTransform.sizeDelta = new Vector2(
                Item.Width.Value * _cellSize,
                Item.Height.Value * _cellSize
            );
        }

        private void UpdateStack()
        {
            if (stackText != null)
            {
                stackText.text = _currentStack.CurrentValue.ToString();
            }
        }

        private Vector2Int CalculateGridPosition(Vector2 screenPosition, InventoryGridView gridView)
        {
            // Получаем RectTransform сетки инвентаря
            RectTransform gridRectTransform = gridView.GridContainer;

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

                if (result.gameObject.TryGetComponent(out EquipmentSlotView slotView))
                {
                    targetView = slotView;
                }
            }

            return targetView;
        }
    }
}