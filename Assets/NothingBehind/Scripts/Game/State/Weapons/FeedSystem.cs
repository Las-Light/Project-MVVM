using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using R3;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class FeedSystem
    {
        public FeedSystemData Origin { get; }
        public ReactiveProperty<MagazinesItem> MagazinesItem;

        public FeedSystem(FeedSystemData data)
        {
            Origin = data;
            MagazinesItem = new ReactiveProperty<MagazinesItem>(new MagazinesItem(data.MagazinesItemData));

            MagazinesItem.Skip(1).Subscribe(value => data.MagazinesItemData = value.Origin);
        }
    }
}