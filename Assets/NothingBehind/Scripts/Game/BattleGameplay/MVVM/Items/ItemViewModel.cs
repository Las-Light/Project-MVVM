using NothingBehind.Scripts.Game.Settings.Gameplay.Items;
using NothingBehind.Scripts.Game.State.Items;
using R3;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Items
{
    public class ItemViewModel
    {
        public ItemSettings ItemSettings { get; }
        public Item Item { get; }
        public int Id { get; }
        public ItemType ItemType { get; }
        public bool CanRotate { get; }
        public bool IsStackable { get; set; }
        public int Weight { get; }
        public int MaxStackSize { get; set; }
        public ReactiveProperty<bool> IsRotated { get; }
        public ReactiveProperty<int> CurrentStack { get; }
        public ReactiveProperty<int> Width { get; }
        public ReactiveProperty<int> Height { get; }


        public ItemViewModel(Item item, ItemSettings itemSettings)
        {
            ItemSettings = itemSettings;
            Item = item;
            Id = item.Id;
            ItemType = item.ItemType;
            Height = item.Height;
            Width = item.Width;
            Weight = item.Weight;
            CurrentStack = item.CurrentStack;
            CanRotate = item.CanRotate;
            IsRotated = item.IsRotated;
            IsStackable = item.IsStackable;
            MaxStackSize = item.MaxStackSize;
        }
    }
}