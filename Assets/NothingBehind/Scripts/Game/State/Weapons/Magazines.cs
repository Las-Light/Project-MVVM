using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Weapons
{
    public class Magazines
    {
        public MagazinesData Origin;
        public string Caliber { get; }
        public int ClipSize { get; }
        public ReactiveProperty<int> CurrentAmmo { get; }
        public ReactiveProperty<bool> IsEmpty { get; }
        public Magazines(MagazinesData data)
        {
            Origin = data;
            Caliber = data.Caliber;
            ClipSize = data.ClipSize;
            CurrentAmmo = new ReactiveProperty<int>(data.CurrentAmmo);
            IsEmpty = new ReactiveProperty<bool>(data.IsFull);

            CurrentAmmo.Skip(1).Subscribe(value =>
            {
                data.CurrentAmmo = value;
                if (value == 0)
                {
                    IsEmpty.OnNext(true);
                }
                else
                {
                    IsEmpty.OnNext(false);
                }
            });
            IsEmpty.Skip(1).Subscribe(value => data.IsFull = value);
        }
        
        public AddItemAmountResult AddAmmo(AmmoItem ammo)
        {
            if (CurrentAmmo.Value + ammo.CurrentStack.Value > ClipSize)
            {
                var addedAmmoAmount = ClipSize - CurrentAmmo.Value;
                Debug.Log($"Ammo reload - {addedAmmoAmount}");
                CurrentAmmo.Value = ClipSize;
                ammo.CurrentStack.Value -= addedAmmoAmount;
                var needRemove = ammo.CurrentStack.Value == 0;
                return new AddItemAmountResult(ammo.ItemType, ammo.Id, ammo.CurrentStack.Value, addedAmmoAmount, needRemove,
                    true);
            }

            CurrentAmmo.Value += ammo.CurrentStack.Value;
            return new AddItemAmountResult(ammo.ItemType, ammo.Id, ammo.CurrentStack.Value, ammo.CurrentStack.Value, true,
                true);
        }
    }
}