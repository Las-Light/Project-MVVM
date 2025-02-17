using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Inventory
{
    [CreateAssetMenu(fileName = "Item Config", menuName = "Inventory/Item Config", order = 3)]
    public class ItemSettings : ScriptableObject
    {
        public string Id;
        public int Width;
        public int Height;
        public bool CanRotate;
    }
}