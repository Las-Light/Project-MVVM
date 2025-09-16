using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Equipments;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public static class ArsenalDataFactory
    {
        public static ArsenalData CreateArsenalData(EntityType entityType, int ownerId)
        {
            var arsenalData = new ArsenalData
            {
                OwnerId = ownerId,
                OwnerType = entityType,
                CurrentWeaponSlot = SlotType.Weapon1,
                Weapons = new List<WeaponData>()
            };
            return arsenalData;
        }
    }
}