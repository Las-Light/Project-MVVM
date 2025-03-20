using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Equipment
{
    [CreateAssetMenu(fileName = "Equipments Config", menuName = "Equipment/Equipments Config", order = 2)]

    public class EquipmentsSettings: ScriptableObject
    {
        public List<EquipmentSettings> AllEquipments;
    }
}