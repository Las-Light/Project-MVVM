using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Inventories;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.InventorySystem
{
    public class InventoryGridSize : MonoBehaviour
    {
        [SerializeField] private Vector2 newSize;
        private void Start()
        {
            var inventoryGridViews = GetComponentsInChildren<InventoryGridView>();
            newSize = new Vector2();
            foreach (var inventoryGridView in inventoryGridViews)
            {
                newSize.y = inventoryGridView.GetComponent<RectTransform>().sizeDelta.y;
                newSize.x += inventoryGridView.GetComponent<RectTransform>().sizeDelta.x;
            }

            GetComponent<RectTransform>().sizeDelta = newSize;
        }
    }
}