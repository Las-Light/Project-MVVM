using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Inventory
{
    public class ItemDataProxy
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

        public ItemDataProxy(ItemData itemData)
        {
            Origin = itemData;
            Id = itemData.Id;
            ItemType = itemData.ItemType;
            CanRotate = itemData.CanRotate;
            Weight = itemData.Weight;
            IsStackable = itemData.IsStackable;
            MaxStackSize = itemData.MaxStackSize;
            IsRotated = new ReactiveProperty<bool>(itemData.IsRotated);
            CurrentStack = new ReactiveProperty<int>(itemData.CurrentStack);
            Width = new ReactiveProperty<int>(itemData.Width);
            Height = new ReactiveProperty<int>(itemData.Height);

            Width.Subscribe(value => itemData.Width = value);
            Height.Subscribe(value => itemData.Height = value);
            IsRotated.Subscribe(value => itemData.IsRotated = value);
            CurrentStack.Subscribe(value => itemData.CurrentStack = value);
        }
    }
}