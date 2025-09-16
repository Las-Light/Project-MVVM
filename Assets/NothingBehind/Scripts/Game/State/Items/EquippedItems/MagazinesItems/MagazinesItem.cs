using NothingBehind.Scripts.Game.State.Weapons;
using R3;

namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems
{
    public class MagazinesItem : Item
    {
        public MagazinesItemData Origin;
        public Magazines Magazines;
        
        public MagazinesItem(MagazinesItemData magazinesItemData) : base(magazinesItemData)
        {
            Origin = magazinesItemData;
            Magazines = new Magazines(magazinesItemData.Magazines);
        }
    }
}