using System;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    [Serializable]
    public class MagazinesData
    {
        public string Caliber;
        public int ClipSize;
        public int CurrentAmmo;
        public bool IsFull;
    }
}