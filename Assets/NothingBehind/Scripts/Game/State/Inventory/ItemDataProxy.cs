using R3;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    public class ItemDataProxy
    {
        public string Id { get; }
        public ItemData Origin { get; }
        public ReactiveProperty<int> Width { get; }
        public ReactiveProperty<int> Height { get; }
        public ReactiveProperty<bool> IsRotated { get; }
        public bool CanRotate { get; }

        public ItemDataProxy(ItemData itemData)
        {
            Origin = itemData;
            Id = itemData.Id;
            CanRotate = itemData.CanRotate;
            IsRotated = new ReactiveProperty<bool>(itemData.IsRotated);
            Width = new ReactiveProperty<int>(itemData.Width);
            Height = new ReactiveProperty<int>(itemData.Height);

            Width.Subscribe(value => itemData.Width = value);
            Height.Subscribe(value => itemData.Height = value);
            IsRotated.Subscribe(value => itemData.IsRotated = value);
        }

    }
}