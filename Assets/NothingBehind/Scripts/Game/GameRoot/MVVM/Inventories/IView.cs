using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.Inventories
{
    public interface IView
    {
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public RectTransform GetContainer();
        public ItemView GetItemViewAtPosition(Vector2Int position);
        public Vector2Int? GetItemPosition(int itemId);
        Vector2 GetSlotWorldPosition(Vector2Int slotPos);
        Vector2 GetSlotScreenPosition(Vector2Int slotPos);
        public void ClearHighlights();
    }
}