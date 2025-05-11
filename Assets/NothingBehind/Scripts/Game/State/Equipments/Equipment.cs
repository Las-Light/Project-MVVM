using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    public class Equipment
    {
        public int OwnerId { get; }
        public int Width { get; }
        public int Height { get; }
        public EquipmentData Origin { get; }
        public ObservableList<EquipmentSlot> Slots = new ();

        public Equipment(EquipmentData equipmentData)
        {
            Origin = equipmentData;
            OwnerId = equipmentData.OwnerId;
            Width = equipmentData.Width;
            Height = equipmentData.Height;
            equipmentData.Slots.ForEach(data => Slots.Add(new EquipmentSlot(data)));
        }
    }
}