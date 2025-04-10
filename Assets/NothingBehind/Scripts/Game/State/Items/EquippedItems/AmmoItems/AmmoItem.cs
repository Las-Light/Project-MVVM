namespace NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems
{
    public class AmmoItem : Item
    {
        public string Caliber;
        public AmmoItem(AmmoItemData ammoItemData) : base(ammoItemData)
        {
            Caliber = ammoItemData.Caliber;
        }
    }
}