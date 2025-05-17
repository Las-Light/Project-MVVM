using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Equipments;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InventorySystem
{
    public class GridNavigationService
    {
        private readonly List<InventoryGridView> _playerGrids;
        private readonly List<InventoryGridView> _lootGrids;
        private readonly EquipmentView _playerEquipmentView;
        private readonly EquipmentView _lootEquipmentView;
        private readonly GridNavigationConfig _config;

        private readonly Dictionary<InventoryGridView, Vector2[]> _gridPositionsCache = new();
        private readonly Dictionary<EquipmentSlotView, Vector2> _slotPositionsCache = new();

        public GridNavigationService(
            List<InventoryGridView> playerGrids,
            List<InventoryGridView> lootGrids,
            EquipmentView playerEquipmentView,
            EquipmentView lootEquipmentView,
            GridNavigationConfig config)
        {
            _playerGrids = playerGrids;
            _lootGrids = lootGrids;
            _playerEquipmentView = playerEquipmentView;
            _lootEquipmentView = lootEquipmentView;
            _config = config;

            //CacheAllPositions();
        }

        public (IView newView, Vector2Int posAtNewView) GetNextView(NavigationDirection direction,
            Vector2Int currentSlotPosition, IView currentGridView)
        {
            switch (currentGridView)
            {
                case InventoryGridView inventoryGridView:
                    (IView, Vector2Int) nextViewAndPos = new();
                    if (_lootGrids.Contains(inventoryGridView))
                    {
                        nextViewAndPos =
                            GetTargetInventorySection(inventoryGridView, _lootGrids, direction, currentSlotPosition);

                        if (nextViewAndPos.Item1 == null && direction is NavigationDirection.Left)
                        {
                            nextViewAndPos =
                                FindNearestInGridsCached(currentGridView, currentSlotPosition, _playerGrids);
                            
                            if (nextViewAndPos.Item1 == null)
                            {
                                nextViewAndPos = (_playerEquipmentView,
                                    FindNearestInEquipment(currentGridView, currentSlotPosition, _playerEquipmentView));
                            }
                        }
                        if (_lootEquipmentView != null 
                            && nextViewAndPos.Item1 == null 
                            && direction is NavigationDirection.Right)
                        {
                            nextViewAndPos = (_lootEquipmentView,
                                FindNearestInEquipment(currentGridView, currentSlotPosition, _lootEquipmentView));
                        }
                    }

                    if (_playerGrids.Contains(inventoryGridView))
                    {
                        nextViewAndPos =
                            GetTargetInventorySection(inventoryGridView, _playerGrids, direction, currentSlotPosition);
                        if (nextViewAndPos.Item1 == null)
                        {
                            if (direction is NavigationDirection.Right)
                            {
                                nextViewAndPos =
                                    FindNearestInGridsCached(currentGridView, currentSlotPosition, _lootGrids);
                                if (_lootEquipmentView != null 
                                    && nextViewAndPos.Item1 == null)
                                {
                                    nextViewAndPos = (_lootEquipmentView,
                                        FindNearestInEquipment(currentGridView, currentSlotPosition, _lootEquipmentView));
                                }
                            }

                            if (direction is NavigationDirection.Left)
                            {
                                nextViewAndPos = (_playerEquipmentView,
                                    FindNearestInEquipment(currentGridView, currentSlotPosition, _playerEquipmentView));
                            }
                        }
                    }

                    if (nextViewAndPos.Item1 != null)
                    {
                        return nextViewAndPos;
                    }

                    break;

                case EquipmentView equipmentView:
                    if (equipmentView == _playerEquipmentView && direction == NavigationDirection.Right)
                    {
                        (IView, Vector2Int) nextInventoryViewAndPos = FindNearestInGridsCached(currentGridView, currentSlotPosition, _playerGrids);
                        if (nextInventoryViewAndPos.Item1 == null)
                        {
                            nextInventoryViewAndPos = FindNearestInGridsCached(currentGridView, currentSlotPosition,
                                _lootGrids);
                        }

                        return nextInventoryViewAndPos;
                    }

                    if (equipmentView == _lootEquipmentView && direction == NavigationDirection.Left)
                    {
                        (IView, Vector2Int) nextInventoryViewAndPos = FindNearestInGridsCached(currentGridView, currentSlotPosition, _lootGrids);
                        if (nextInventoryViewAndPos.Item1 == null)
                        {
                            nextInventoryViewAndPos = FindNearestInGridsCached(currentGridView, currentSlotPosition,
                                _playerGrids);
                        }

                        return nextInventoryViewAndPos;
                    }

                    break;
            }

            return (null, Vector2Int.zero);
        }

        private (IView newView, Vector2Int posAtNewView) GetTargetInventorySection(InventoryGridView currentGrid,
            List<InventoryGridView> allInventoryGridViews,
            NavigationDirection direction,
            Vector2Int currentSlotPos)
        {
            if (currentGrid == null) return (null, Vector2Int.zero);

            // Ищем подходящее правило перехода
            var transition = FindTransition(currentGrid.GridType, direction);

            if (transition == null) return (null, Vector2Int.zero);

            // Фильтруем сетки по целевому типу
            var targetGrids = allInventoryGridViews
                .Where(g => g.GridType == transition.TargetType)
                .OrderBy(g => g.GridIndex)
                .ToList();

            if (!targetGrids.Any()) return (null, Vector2Int.zero);

            // Если переход между сетками одного типа с изменением индекса
            if (transition.SameTypeTransition && transition.SourceType == transition.TargetType)
            {
                int indexDelta = direction == NavigationDirection.Right ? 1 : -1;
                int targetIndex = currentGrid.GridIndex + indexDelta;

                // Проверяем границы допустимых индексов
                if (targetIndex >= 0 && targetIndex < targetGrids.Count)
                {
                    var sourceWorldPos = currentGrid.GetSlotWorldPosition(currentSlotPos);
                    var targetPos = FindNearestInGrid(sourceWorldPos, targetGrids[targetIndex]);
                    return (targetGrids[targetIndex], targetPos);
                }

                return (null, Vector2Int.zero);
            }

            // Для вертикальных переходов между разными типами
            if (!transition.SameTypeTransition)
            {
                return FindNearestInGridsCached(currentGrid, currentSlotPos, targetGrids);
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

        private (InventoryGridView grid, Vector2Int slotPos) FindNearestInGridsCached(IView sourceView,
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
                if (!_gridPositionsCache.TryGetValue(grid, out var worldPositions))
                {
                    worldPositions = PrecalculateGridPositions(grid);
                    _gridPositionsCache[grid] = worldPositions;
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

        private GridTransition FindTransition(InventoryGridType sourceType,
            NavigationDirection direction)
        {
            return _config.Transitions.FirstOrDefault(t =>
                t.SourceType == sourceType &&
                t.Direction == direction);
        }

        private Vector2Int FindNearestInEquipment(IView sourceView,
            Vector2Int sourceSlotPos,
            EquipmentView equipmentView)
        {
            if (_slotPositionsCache.Count == 0)
            {
                CacheEquipmentPositions();
            }
            var sourceWorldPos = sourceView.GetSlotWorldPosition(sourceSlotPos);
            EquipmentSlotView nearestSlot = null;
            float minDistance = float.MaxValue;

            foreach (var (slot, pos) in _slotPositionsCache)
            {
                float distance = Vector2.Distance(sourceWorldPos, pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSlot = slot;
                }
            }

            return nearestSlot != null ? equipmentView.GetSlotPosition(nearestSlot) : Vector2Int.zero;
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

        private void CacheEquipmentPositions()
        {
            _slotPositionsCache.Clear();
            foreach (var slot in _playerEquipmentView.AllSlotViews)
            {
                _slotPositionsCache[slot] = slot.transform.position;
            }
        }

        private void CacheAllPositions()
        {
            foreach (var grid in _playerGrids.Concat(_lootGrids))
            {
                _gridPositionsCache[grid] = PrecalculateGridPositions(grid);
            }

            CacheEquipmentPositions();
        }

        public void RefreshCache()
        {
            _gridPositionsCache.Clear();
            _slotPositionsCache.Clear();
            CacheAllPositions();
        }
    }
}