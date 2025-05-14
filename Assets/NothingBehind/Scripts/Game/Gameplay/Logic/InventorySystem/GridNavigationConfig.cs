using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InventorySystem
{
    [Serializable]
    public class GridNavigationConfig
    {
        [Serializable]
        public class GridTransition
        {
            public InventoryGridType SourceType;
            public NavigationDirection Direction;
            public InventoryGridType TargetType;
            public bool IsTransToAnotherType = true;
        }

        public List<GridTransition> Transitions = new()
        {
            // Стандартные переходы
            new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Down, TargetType = InventoryGridType.Backpack, IsTransToAnotherType = true},
            new() { SourceType = InventoryGridType.Backpack, Direction = NavigationDirection.Up, TargetType = InventoryGridType.ChestRig, IsTransToAnotherType = true},
            new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Right, TargetType = InventoryGridType.ChestRig, IsTransToAnotherType = false },
            new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Left, TargetType = InventoryGridType.ChestRig, IsTransToAnotherType = false }
        };
    }
}