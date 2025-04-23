using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic.WeaponSystem;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Inventories;
using NothingBehind.Scripts.Game.Settings.Gameplay.Weapons;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.AmmoItems;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.Game.State.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons
{
    public class WeaponViewModel
    {
        public int Id { get; }
        public WeaponName WeaponName { get; }
        public ImpactType ImpactType { get; }
        public WeaponType WeaponType { get; }
        public string Caliber { get; set; }
        public GameObject ModelPrefab { get; }
        public float CheckDistanceToWall { get; }
        public float AimingRange { get; }
        public Vector3 SpawnPoint { get; }
        public Vector3 SpawnRotation { get; }
        public Damage Damage { get; }
        public Shoot Shoot { get; }
        public FeedSystem FeedSystem { get; }
        public BulletPenetration BulletPenetration { get; }
        public Knockback Knockback { get; }

        public TrailData Trail { get; }
        public AudioWeapon AudioWeapon { get; }
        public readonly ReactiveProperty<int> AppropriateMagazinesCount = new();
        public readonly ReactiveProperty<int> AppropriateAmmoCount = new();

        private readonly ObservableDictionary<InventoryGridViewModel, ObservableList<AmmoItem>>
            _appropriateAmmo = new();

        private readonly ObservableDictionary<InventoryGridViewModel, ObservableList<MagazinesItem>>
            _appropriateMagazines = new();

        private IReadOnlyObservableDictionary<InventoryGridViewModel, ObservableList<MagazinesItem>> _allMagazines;
        private IReadOnlyObservableDictionary<InventoryGridViewModel, ObservableList<AmmoItem>> _allAmmo;

        private CompositeDisposable _disposables = new();


        public WeaponViewModel(Weapon weapon, WeaponSettings weaponSettings, ArsenalViewModel arsenalViewModel)
        {
            Id = weapon.Id;
            WeaponName = weapon.WeaponName;
            ImpactType = weapon.ImpactType;
            WeaponType = weapon.WeaponType;
            Caliber = weapon.Caliber;
            ModelPrefab = weaponSettings.ModelPrefab;
            AimingRange = weapon.AimingRange.Value;
            CheckDistanceToWall = weapon.CheckDistanceToWall.Value;
            SpawnPoint = weapon.SpawnPoint.Value;
            SpawnRotation = weapon.SpawnRotation.Value;
            Damage = weapon.Damage.Value;
            Shoot = weapon.Shoot.Value;
            Shoot.SpreadTexture = weaponSettings.shootSettings.SpreadTexture;
            Shoot.BulletPrefab = weaponSettings.shootSettings.BulletPrefab;
            FeedSystem = weapon.FeedSystem.Value;
            BulletPenetration = weapon.BulletPenetration.Value;
            Knockback = weapon.Knockback.Value;
            Knockback.DistanceFalloff = weaponSettings.knockbackSettings.DistanceFalloff;
            Trail = CreateTrailData(weaponSettings.trailSettings);
            AudioWeapon = CreateAudioWeaponData(weaponSettings.audioSettings);

            _allMagazines = arsenalViewModel.AllMagazines;
            _allAmmo = arsenalViewModel.AllAmmo;

            foreach (var kvp in _allAmmo)
            {
                _appropriateAmmo[kvp.Key] = new ObservableList<AmmoItem>();
                if (kvp.Value.Count > 0)
                {
                    foreach (var ammoItem in kvp.Value)
                    {
                        if (ammoItem.Caliber == Caliber)
                        {
                            AppropriateAmmoCount.Value += ammoItem.CurrentStack.Value;
                            _appropriateAmmo[kvp.Key].Add(ammoItem);
                        }
                    }
                }

                _disposables.Add(kvp.Value.ObserveAdd().Subscribe(e =>
                {
                    var addedAmmo = e.Value;
                    if (_appropriateAmmo.TryGetValue(kvp.Key, out var ammoItems))
                    {
                        if (addedAmmo.Caliber == Caliber)
                        {
                            AppropriateAmmoCount.Value += addedAmmo.CurrentStack.Value;
                            ammoItems.Add(addedAmmo);
                        }
                    }
                    else
                    {
                        AppropriateAmmoCount.Value += addedAmmo.CurrentStack.Value;
                        _appropriateAmmo[kvp.Key] = new ObservableList<AmmoItem> { addedAmmo };
                    }
                }));

                _disposables.Add(kvp.Value.ObserveRemove().Subscribe(e =>
                {
                    var removedAmmo = e.Value;
                    if (_appropriateAmmo.TryGetValue(kvp.Key, out var ammoItems))
                    {
                        //AppropriateAmmoCount.Value -= removedAmmo.CurrentStack.Value;
                        ammoItems.Remove(removedAmmo);
                    }
                }));
            }

            foreach (var kvp in _allMagazines)
            {
                _appropriateMagazines[kvp.Key] = new ObservableList<MagazinesItem>();
                if (kvp.Value.Count > 0)
                {
                    foreach (var magazinesItem in kvp.Value)
                    {
                        if (magazinesItem.Magazines.Caliber == Caliber)
                        {
                            AppropriateMagazinesCount.Value++;
                            _appropriateMagazines[kvp.Key].Add(magazinesItem);
                        }
                    }
                }

                _disposables.Add(kvp.Value.ObserveAdd().Subscribe(e =>
                {
                    var addedMagazines = e.Value;
                    if (_appropriateMagazines.TryGetValue(kvp.Key, out var magazinesItems))
                    {
                        if (addedMagazines.Magazines.Caliber == Caliber)
                        {
                            AppropriateMagazinesCount.Value++;
                            magazinesItems.Add(addedMagazines);
                        }
                    }
                    else
                    {
                        AppropriateMagazinesCount.Value++;
                        _appropriateMagazines[kvp.Key] = new ObservableList<MagazinesItem> { addedMagazines };
                    }
                }));

                _disposables.Add(kvp.Value.ObserveRemove().Subscribe(e =>
                {
                    var removedMagazines = e.Value;
                    if (removedMagazines.Magazines.Caliber == Caliber)
                    {
                        if (_appropriateMagazines.TryGetValue(kvp.Key, out var magazinesItems))
                        {
                            AppropriateMagazinesCount.Value--;
                            magazinesItems.Remove(removedMagazines);
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// Reloads with the ammo conserving algorithm.
        /// </summary>
        public bool Reload()
        {
            if (!ChangeMagazine())
            {
                if (!LoadMagazines())
                {
                    return false;
                }

                return true;
            }

            return true;
        }

        public bool LoadMagazines()
        {
            var magazines = FeedSystem.MagazinesItem.Value.Magazines;
            if (AppropriateAmmoCount.Value > 0)
            {
                foreach (var kvp in _appropriateAmmo)
                {
                    if (kvp.Value.Count > 0)
                    {
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            var addedAmmoResult = magazines.AddAmmo(kvp.Value[i]);
                            AppropriateAmmoCount.Value -= addedAmmoResult.ItemsAddedAmount;
                            if (addedAmmoResult.NeedRemove)
                            {
                                kvp.Key.RemoveItem(kvp.Value[i].Id);
                            }

                            if (magazines.CurrentAmmo.Value == magazines.ClipSize || AppropriateAmmoCount.Value <= 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool ChangeMagazine()
        {
            if (_appropriateMagazines.Count > 0)
            {
                foreach (var kvp in _appropriateMagazines)
                {
                    if (kvp.Value.Count > 0)
                    {
                        foreach (var magazinesItem in kvp.Value)
                        {
                            if (!magazinesItem.Magazines.IsEmpty.Value)
                            {
                                var magazinesInWeapon = FeedSystem.MagazinesItem.Value;
                                _appropriateMagazines[kvp.Key].Add(magazinesInWeapon);
                                var replaceItemPos = kvp.Key.GetItemPosition(magazinesItem.Id);
                                if (replaceItemPos != null)
                                {
                                    kvp.Key.RemoveItem(magazinesItem.Id);
                                    kvp.Key.AddItems(magazinesInWeapon, replaceItemPos.Value,
                                        magazinesItem.CurrentStack.Value);
                                    _appropriateMagazines[kvp.Key].Remove(magazinesItem);
                                    FeedSystem.MagazinesItem.Value = magazinesItem;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool CanReload()
        {
            bool isHaveFullMagazines = false;
            if (AppropriateMagazinesCount.Value > 0)
            {
                isHaveFullMagazines = _appropriateMagazines
                    .Where(kvp => kvp.Value.Count > 0)
                    .SelectMany(kvp => kvp.Value)
                    .Any(magazinesItem => !magazinesItem.Magazines.IsEmpty.Value);
            }

            return AppropriateAmmoCount.Value > 0 || isHaveFullMagazines;
        }

        private AudioWeapon CreateAudioWeaponData(AudioWeaponSettings audioSettings)
        {
            var audioWeaponData = new AudioWeapon
            {
                Volume = audioSettings.Volume,
                FireClips = audioSettings.FireClips,
                EmptyClip = audioSettings.EmptyClip,
                ReloadClip = audioSettings.ReloadClip,
                LastBulletClip = audioSettings.LastBulletClip
            };
            return audioWeaponData;
        }

        private TrailData CreateTrailData(TrailSettings trailSettings)
        {
            var trailData = new TrailData
            {
                Material = trailSettings.Material,
                WidthCurve = trailSettings.WidthCurve,
                Duration = trailSettings.Duration,
                MinVertexDistance = trailSettings.MinVertexDistance,
                Color = trailSettings.Color,
                MissDistance = trailSettings.MissDistance,
                SimulationSpeed = trailSettings.SimulationSpeed
            };
            return trailData;
        }
    }
}