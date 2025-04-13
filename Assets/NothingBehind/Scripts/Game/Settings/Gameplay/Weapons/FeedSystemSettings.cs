using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "FeedSystem Settings", menuName = "Guns/FeedSystem Settings", order = 4)]
    public class FeedSystemSettings : ScriptableObject
    {
        public ItemSettings MagazinesItemSettings;
    }
}