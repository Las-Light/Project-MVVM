using System;
using NothingBehind.Scripts.Game.State.Inventories.Grids;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InventorySystem
{
    [Serializable]
    public class GridTransition
    {
        public InventoryGridType SourceType;
        public NavigationDirection Direction;
        public InventoryGridType TargetType;

        [Tooltip("For transitions between same type grids")]
        public bool SameTypeTransition;
    }
}