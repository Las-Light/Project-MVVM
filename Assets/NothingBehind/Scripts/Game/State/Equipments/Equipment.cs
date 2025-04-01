using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    public class Equipment
    {
        public int OwnerId { get; }
        public EquipmentData Origin { get; }
        public ObservableList<EquipmentSlot> Slots = new ();

        public Equipment(EquipmentData equipmentData)
        {
            Origin = equipmentData;
            OwnerId = equipmentData.OwnerId;
            equipmentData.Slots.ForEach(data => Slots.Add(new EquipmentSlot(data)));
        }
    }
}