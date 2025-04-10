using System;
using System.Collections.Generic;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class ArsenalData
    {
        public int OwnerId;
        public List<WeaponData> Weapons;
    }
}