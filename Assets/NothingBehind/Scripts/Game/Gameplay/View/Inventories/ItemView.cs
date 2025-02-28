using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.View.Inventories
{
    public class ItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text stackText;
        private ItemDataProxy _item;
        private float _cellSize;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Image _itemImage;
        private Canvas _mainCanvas;
        private RectTransform _canvasRectTransform;

        private Vector2 _startPosition;
        private Transform _startParent;
        private InventoryGridView _startInventoryGridView;
        private InventoryView _startInventoryView;
        private InventoryGridView _currentGridView;
        private Vector2 _offset; // Смещение между позицией курсора и центром предмета

        public void Initialize(ItemDataProxy item, float cellSize)
        {
            _item = item;
            _cellSize = cellSize;

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _itemImage = GetComponent<Image>();
            _mainCanvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = _mainCanvas.transform as RectTransform;

            UpdateVisuals();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Запоминаем начальную позицию и родителя
            _startPosition = _rectTransform.anchoredPosition;
            var parent = _rectTransform.parent;
            _startParent = parent;
            
            // Получаем целевой инвентарь (через Raycast)
            var targetViews = GetTargetInventoryView(eventData);

            if (targetViews?.Item1 != null && targetViews?.Item2 != null)
            {
                _startInventoryGridView = targetViews.Value.Item1;
                _startInventoryView = targetViews.Value.Item2;
            }

            // Отключаем блокировку raycast, чтобы предмет можно было перетаскивать
            _canvasGroup.blocksRaycasts = false;
            
            _rectTransform.SetParent(_canvasRectTransform);

            // Перемещаем предмет на верхний слой
            _rectTransform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Перемещаем предмет за курсором
            _rectTransform.anchoredPosition += eventData.delta/_mainCanvas.scaleFactor;

            // Получаем целевой инвентарь (через Raycast)
            var targetViews = GetTargetInventoryView(eventData);
            
            // Обновляем подсветку
            if (targetViews?.Item1 != null && targetViews?.Item2 != null)
            {
                var targetInventoryGridView = targetViews.Value.Item1;
                if (_currentGridView != null && _currentGridView != targetInventoryGridView)
                {
                    _currentGridView.ClearHighlights();
                }
                _currentGridView = targetInventoryGridView;
                var gridPos = CalculateGridPosition(eventData.position, targetInventoryGridView);
                targetInventoryGridView.UpdateHighlights(_item, gridPos);
            }
            else
            {
                _currentGridView.ClearHighlights();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Включаем блокировку raycast обратно
            _canvasGroup.blocksRaycasts = true;

            // Получаем целевой инвентарь (через Raycast)
            var targetViews = GetTargetInventoryView(eventData);
            
            if (targetViews?.Item1 != null && targetViews?.Item2 != null)
            {
                var targetInventoryGridView = targetViews.Value.Item1;
                var targetInventoryView = targetViews.Value.Item2;
                _currentGridView = targetInventoryGridView;
                
                var targetInventoryViewModel = targetInventoryView.GetInventoryViewModel();
                var targetInventoryGridViewModel = targetInventoryGridView.GetGridViewModel();
                
                // Преобразуем позицию курсора в координаты сетки целевого инвентаря
                var gridPos = CalculateGridPosition(eventData.position, targetInventoryGridView);

                bool success;
                
                if (targetInventoryView != _startInventoryView)
                {
                    var startInventoryViewModel = _startInventoryView.GetInventoryViewModel();
                    var startInventoryGridViewModel = _startInventoryGridView.GetGridViewModel();
                    success = targetInventoryViewModel.TryMoveItemToAnotherInventory(startInventoryViewModel.OwnerId,
                        targetInventoryViewModel.OwnerId,
                        startInventoryGridViewModel.GridTypeID, targetInventoryGridViewModel.GridTypeID, _item, gridPos,
                        _item.CurrentStack.Value).Success;
                }
                else
                {
                    if (targetInventoryGridView != _startInventoryGridView)
                    {
                        var startGridViewModel = _startInventoryGridView.GetGridViewModel();
                        success = targetInventoryViewModel.TryMoveItemToAnotherGrid(startGridViewModel.GridTypeID,
                            targetInventoryGridViewModel.GridTypeID,
                            _item, gridPos, _item.CurrentStack.Value).Success;
                    
                    }
                    else
                    {
                        // Пытаемся передать предмет в целевой инвентарь
                        success = targetInventoryViewModel.TryMoveItemInGrid(targetInventoryGridViewModel.GridTypeID,
                            _item, gridPos, _item.CurrentStack.Value).Success;
                    }
                }
                if (success)
                {
                    _rectTransform.SetParent(targetInventoryGridView.transform);
                    // Устанавливаем позицию предмета на основе координат ячейки сетки
                    Vector2 cellPosition = new Vector2(
                        gridPos.x * targetInventoryGridView.CellSize,
                        -gridPos.y * targetInventoryGridView.CellSize
                    );
                    _rectTransform.anchoredPosition = cellPosition;
                }
                else
                {
                    // Если передача не удалась, возвращаем предмет на исходную позицию
                    _rectTransform.SetParent(_startParent);
                    _rectTransform.anchoredPosition = _startPosition;
                }
                targetInventoryGridView.ClearHighlights();
            }
            else
            {
                // Если целевой инвентарь не найден, возвращаем предмет на исходную позицию
                _rectTransform.SetParent(_startParent);
                _rectTransform.anchoredPosition = _startPosition;
                _currentGridView.ClearHighlights();
            }
        }

        private void UpdateVisuals()
        {
            // Устанавливаем размер предмета в соответствии с его шириной и высотой
            _rectTransform.sizeDelta = new Vector2(
                _item.Width.Value * _cellSize,
                _item.Height.Value * _cellSize
            );

            // Устанавливаем иконку предмета (предположим, что она хранится в ItemModel)
            //TODO: сделать подгрузку картинки из ItemSettings
            //_itemImage.sprite = _item.Icon;

            // Если предмет стакаемый, отображаем количество
            if (_item.IsStackable)
            {
                if (stackText != null)
                {
                    stackText.text = _item.CurrentStack.Value.ToString();
                }
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

        private (InventoryGridView,InventoryView)? GetTargetInventoryView(PointerEventData eventData)
        {
            (InventoryGridView, InventoryView) targetView = new();
            
            GraphicRaycaster ray = GetComponentInParent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            ray.Raycast(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent(out InventoryGridView gridView))
                {
                    targetView.Item1 = gridView;
                }

                if (result.gameObject.TryGetComponent(out InventoryView inventoryView))
                {
                    targetView.Item2 = inventoryView;
                }
            }

            return targetView;
        }
    }
}