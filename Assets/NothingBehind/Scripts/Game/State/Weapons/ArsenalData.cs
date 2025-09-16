using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class ArsenalData
    {
        public int OwnerId;
        public EntityType OwnerType;
        public SlotType CurrentWeaponSlot;
        public List<WeaponData> Weapons;
    }
}