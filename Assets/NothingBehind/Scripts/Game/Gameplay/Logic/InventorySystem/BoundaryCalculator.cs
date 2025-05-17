using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.InventorySystem
{
    public static class BoundaryCalculator
    {
        public static Vector2Int Calculate(
            Vector2Int newPosition,
            NavigationDirection direction,
            int left, int right, int top, int bottom)
        {
            return direction switch
            {
                NavigationDirection.Right => new Vector2Int(right + 1, Mathf.Clamp(newPosition.y, top, bottom)),
                NavigationDirection.Left => new Vector2Int(left - 1, Mathf.Clamp(newPosition.y, top, bottom)),
                NavigationDirection.Down => new Vector2Int(Mathf.Clamp(newPosition.x, left, right), bottom + 1),
                _ => new Vector2Int(Mathf.Clamp(newPosition.x, left, right), top - 1)
            };
        }
        public static Vector2Int CalculateEquipBoundary(
            Vector2Int newPosition,
            NavigationDirection direction,
            int left, int right, int top, int bottom)
        {
            return direction switch
            {
                NavigationDirection.Right => new Vector2Int(right, Mathf.Clamp(newPosition.y, top, bottom)),
                NavigationDirection.Left => new Vector2Int(left, Mathf.Clamp(newPosition.y, top, bottom)),
                NavigationDirection.Down => new Vector2Int(Mathf.Clamp(newPosition.x, left, right), bottom),
                _ => new Vector2Int(Mathf.Clamp(newPosition.x, left, right), top)
            };
        }
    }
}