using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Equipments;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class ArsenalData
    {
        public int OwnerId;
        public SlotType CurrentWeaponSlot;
        public List<WeaponData> Weapons;
    }
}