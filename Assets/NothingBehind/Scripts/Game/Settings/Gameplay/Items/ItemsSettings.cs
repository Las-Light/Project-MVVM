using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Items
{
    [CreateAssetMenu(fileName = "Items Config", menuName = "Items/Items Config", order = 0)]
    public class ItemsSettings : ScriptableObject
    {
        public List<ItemSettings> Items;
    }
}