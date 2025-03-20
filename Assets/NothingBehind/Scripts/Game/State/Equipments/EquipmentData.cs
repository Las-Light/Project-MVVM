using System;
using System.Collections.Generic;

namespace NothingBehind.Scripts.Game.State.Equipments
{
    [Serializable]
    public class EquipmentData
    {
        public int OwnerId;
        public List<EquipmentSlotData> Slots;
    }
}