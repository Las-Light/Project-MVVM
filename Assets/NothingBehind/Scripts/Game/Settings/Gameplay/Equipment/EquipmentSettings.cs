using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Equipment
{
    [CreateAssetMenu(fileName = "Equipment Config", menuName = "Equipment/Equipment Config", order = 0)]

    public class EquipmentSettings: ScriptableObject
    {
        public EntityType EntityType;
        public int Width;
        public int Height;
        public List<EquipmentSlotSettings> Slots;
    }
}