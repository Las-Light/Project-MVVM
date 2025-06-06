using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Inventories.Grids;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.InventorySystem
{
    [Serializable]
    public class GridNavigationConfig
    {
        public List<GridTransition> Transitions;

        public GridNavigationConfig()
        {
            SetTransition();
        }

        private void SetTransition()
        {
            Transitions = new()
            {
                // Стандартные переходы
                new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Down, TargetType = InventoryGridType.Backpack, SameTypeTransition = false},
                new() { SourceType = InventoryGridType.Backpack, Direction = NavigationDirection.Up, TargetType = InventoryGridType.ChestRig, SameTypeTransition = false},
                new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Right, TargetType = InventoryGridType.ChestRig, SameTypeTransition = true },
                new() { SourceType = InventoryGridType.ChestRig, Direction = NavigationDirection.Left, TargetType = InventoryGridType.ChestRig, SameTypeTransition = true },
            };
        }
    }
}