using R3;

namespace NothingBehind.Scripts.Game.State.Items
{
    public abstract class Item
    {
        public int Id { get; }
        public ItemType ItemType { get; }
        public ItemData Origin { get; }
        public ReactiveProperty<int> Width { get; }
        public ReactiveProperty<int> Height { get; }
        public ReactiveProperty<bool> IsRotated { get; }
        public ReactiveProperty<int> CurrentStack { get; }
        public bool CanRotate { get; }
        public bool IsStackable { get; }
        public int MaxStackSize { get; }
        public int Weight { get; }

        public Item(ItemData ammoItemData)
        {
            Origin = ammoItemData;
            Id = ammoItemData.Id;
            ItemType = ammoItemData.ItemType;
            CanRotate = ammoItemData.CanRotate;
            Weight = ammoItemData.Weight;
            IsStackable = ammoItemData.IsStackable;
            MaxStackSize = ammoItemData.MaxStackSize;
            IsRotated = new ReactiveProperty<bool>(ammoItemData.IsRotated);
            CurrentStack = new ReactiveProperty<int>(ammoItemData.CurrentStack);
            Width = new ReactiveProperty<int>(ammoItemData.Width);
            Height = new ReactiveProperty<int>(ammoItemData.Height);

            Width.Subscribe(value => ammoItemData.Width = value);
            Height.Subscribe(value => ammoItemData.Height = value);
            IsRotated.Subscribe(value => ammoItemData.IsRotated = value);
            CurrentStack.Subscribe(value => ammoItemData.CurrentStack = value);
        }
    }
}