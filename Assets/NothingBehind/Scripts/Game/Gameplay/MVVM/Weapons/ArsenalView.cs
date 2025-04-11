using System;
using System.Collections;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.Sound;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons
{
    public class ArsenalView : MonoBehaviour
    {
        [SerializeField] private GameObject _weaponViewPrefab;
        public IReadOnlyObservableList<WeaponViewModel> AllWeaponViewModels;
        public IReadOnlyObservableDictionary<SlotType, Item> EquippedItems;
        public bool IsReloading => _reloading;

        public WeaponView WeaponSlot1;
        public WeaponView WeaponSlot2;
        private WeaponView _unarmedView;
        private WeaponView _activeGun;
        private Transform _pistolParent;
        private Transform _rifleParent;
        private Transform _unarmedParent;
        private bool autoReload = false;

        private readonly Dictionary<int, WeaponView> _weaponViewMap = new();


        /// <summary>
        /// If you are not using the demo AttachmentController, you may want it to initialize itself on start.
        /// If you are configuring this separately using <see cref="SetupWeapon"/> then set this to false.
        /// </summary>
        [SerializeField] private bool initializeOnStart = false;

        private RigController _rigController;
        private AnimatorController _animatorController;
        private SoundController _soundController;
        private Transform _gunParent;

        private bool _isShootTimerStart;
        private float _shootTimer;
        private bool _reloading;

        private CompositeDisposable _disposables = new();

        public void Bind(ArsenalViewModel viewModel, Transform pistolParent, Transform rifleParent,
            Transform unarmedParent)
        {
            AllWeaponViewModels = viewModel.AllWeaponViewModels;
            EquippedItems = viewModel.EquipmentItems;

            _animatorController = GetComponentInParent<AnimatorController>();
            _rigController = GetComponentInParent<RigController>();
            _soundController = GetComponentInParent<SoundController>();
            _pistolParent = pistolParent;
            _rifleParent = rifleParent;
            _unarmedParent = unarmedParent;

            foreach (var weaponViewModel in AllWeaponViewModels)
            {
                if (weaponViewModel.WeaponType == WeaponType.Unarmed)
                {
                    _unarmedView = SetupWeapon(weaponViewModel);
                }
                else
                {
                    SetupWeapon(weaponViewModel);
                }
            }

            foreach (var kvp in EquippedItems)
            {
                if (kvp.Key is SlotType.Weapon1 or SlotType.Weapon2)
                {
                    if (_weaponViewMap.TryGetValue(kvp.Value.Id, out var weaponView))
                    {
                        switch (kvp.Key)
                        {
                            case SlotType.Weapon1:
                                WeaponSlot1 = weaponView;
                                break;
                            case SlotType.Weapon2:
                                WeaponSlot2 = weaponView;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"WeaponView not found for weapon with Id {kvp.Value.Id}");
                    }
                }
            }

            _disposables.Add(AllWeaponViewModels.ObserveAdd().Subscribe(e =>
            {
                var addedWeapon = e.Value;
                SetupWeapon(addedWeapon);
            }));

            _disposables.Add(AllWeaponViewModels.ObserveRemove().Subscribe(e =>
            {
                var removedWeapon = e.Value;
                DestroyWeaponView(removedWeapon.Id);
            }));

            _disposables.Add(EquippedItems.ObserveAdd().Subscribe(e =>
            {
                var kvp = e.Value;
                if (kvp.Key is SlotType.Weapon1 or SlotType.Weapon2)
                {
                    if (_weaponViewMap.TryGetValue(kvp.Value.Id, out var weaponView))
                    {
                        switch (kvp.Key)
                        {
                            case SlotType.Weapon1:
                                if (_activeGun == WeaponSlot1) WeaponSwitch(weaponView);
                                WeaponSlot1 = weaponView;
                                break;
                            case SlotType.Weapon2:
                                if (_activeGun == WeaponSlot2) WeaponSwitch(weaponView);
                                WeaponSlot2 = weaponView;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"WeaponView not found for weapon Id {kvp.Value.Id}");
                    }
                }
            }));

            _disposables.Add(EquippedItems.ObserveRemove().Subscribe(e =>
            {
                var kvp = e.Value;
                switch (kvp.Key)
                {
                    case SlotType.Weapon1:
                        if (_activeGun == WeaponSlot1) WeaponSwitch(_unarmedView);
                        WeaponSlot1 = _unarmedView;
                        break;
                    case SlotType.Weapon2:
                        if (_activeGun == WeaponSlot2) WeaponSwitch(_unarmedView);
                        WeaponSlot2 = _unarmedView;
                        break;
                }
            }));

            DefaultCurrentWeapon();

            _animatorController.StateExited += EndReloadDueToInterruption;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void EndReloadDueToInterruption(AnimatorState state)
        {
            if (state == AnimatorState.Reload && _reloading)
            {
                _activeGun.EndReload();
                _reloading = false;
                StartCoroutine(SetRigHand());
            }
        }

        public void EndReload()
        {
            _activeGun.EndReload();
            _reloading = false;
            StartCoroutine(SetRigHand());
        }


        //метод который стреляет при нажатии на кнопку "Стрелять"
        public bool Shoot()
        {
            if (ShouldAutoReload())
            {
                Reload();
                return false;
            }

            _activeGun.Tick(!_reloading && _activeGun != null);
            if (_activeGun.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RifleShootRecoil();
            }

            if (_activeGun.WeaponType == WeaponType.Pistol)
            {
                _animatorController.PistolShootRecoil();
            }

            //_soundController.MakeSoundSelf(ActiveGun.GetRaycastOrigin(), SoundType.Shoot);

            return true;
        }

        public void WeaponSwitch(WeaponView targetWeapon)
        {
            if (_activeGun == targetWeapon)
                return;
            
            switch (targetWeapon.WeaponType)
            {
                case WeaponType.Unarmed:
                    WeaponSwitchUnarmed(targetWeapon);
                    break;
                case WeaponType.Rifle:
                    WeaponSwitchRifle(targetWeapon);
                    break;
                case WeaponType.Pistol:
                    WeaponSwitchPistol(targetWeapon);
                    break;
                case WeaponType.Melee:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WeaponSwitchRifle(WeaponView weaponView)
        {
            if (_activeGun.WeaponType == WeaponType.Pistol)
            {
                _animatorController.RigPutPistol();
            }

            if (_activeGun.WeaponType == WeaponType.Rifle && _activeGun != weaponView)
            {
                _animatorController.RigPutRifle();
            }

            StartCoroutine(GoStateGetRifle(weaponView));
        }


        private void WeaponSwitchPistol(WeaponView weaponView)
        {
            if (_activeGun.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }

            if (_activeGun.WeaponType == WeaponType.Pistol && _activeGun != weaponView)
            {
                _animatorController.RigPutPistol();
            }

            if (_activeGun.WeaponType != WeaponType.Pistol)
            {
                StartCoroutine(GoStateGetPistol(weaponView));
            }
        }


        private void WeaponSwitchUnarmed(WeaponView weaponView)
        {
            if (_activeGun.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }
            else if (_activeGun.WeaponType == WeaponType.Pistol)
            {
                _animatorController.RigPutPistol();
            }

            StartCoroutine(GoStateUnarmed(weaponView));
        }

        private WeaponView SetupWeapon(WeaponViewModel weaponViewModel)
        {
            switch (weaponViewModel.WeaponType)
            {
                case WeaponType.Unarmed:
                    _gunParent = _unarmedParent;
                    break;
                case WeaponType.Rifle:
                    _gunParent = _rifleParent;
                    break;
                case WeaponType.Pistol:
                    _gunParent = _pistolParent;
                    break;
                case WeaponType.Melee:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var weapon = Instantiate(_weaponViewPrefab, _gunParent);
            var weaponView = weapon.GetComponent<WeaponView>();
            weaponView.Bind(weaponViewModel);
            _weaponViewMap[weaponViewModel.Id] = weaponView;
            return weaponView;
        }

        private void DestroyWeaponView(int weaponId)
        {
            if (_weaponViewMap.TryGetValue(weaponId, out var weaponView))
            {
                if (weaponView == _activeGun)
                {
                    DespawnActiveGun();
                }

                _weaponViewMap.Remove(weaponId);
                Destroy(weaponView.gameObject);
            }
        }

        private void DespawnActiveGun()
        {
            if (_activeGun != null)
            {
                _activeGun.Despawn();
            }
        }

        // public void ApplyModifiers(IModifier[] Modifiers)
        // {
        //     DespawnActiveGun();
        //     SetupGun(ActiveBaseGun);
        //
        //     foreach (IModifier modifier in Modifiers)
        //     {
        //         modifier.Apply(ActiveGun);
        //     }
        // }

        public void Reload()
        {
            if (ShouldManualReload() || ShouldAutoReload())
            {
                _activeGun.StartReloading();
                _reloading = true;
                _animatorController.Reload();
                _rigController.SetRigLayerLeftHandIK(0, _activeGun.WeaponType);
            }
        }

        //корутина нужна для завершения анимации смены оружия
        private IEnumerator GoStateGetPistol(WeaponView weaponView)
        {
            if (_activeGun != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun = weaponView;
            _activeGun.Spawn();

            _animatorController.RigGetPistol();
            _rigController.SetRigWeightGetPistol();
            _animatorController.GetPistol();

            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateGetRifle(WeaponView weaponView)
        {
            if (_activeGun != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun = weaponView;
            _activeGun.Spawn();

            _animatorController.RigGetRifle();
            _rigController.SetRigWeightGetRifle();
            _animatorController.GetRifle();
            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateUnarmed(WeaponView weaponView)
        {
            yield return new WaitForSeconds(0.3f);
            if (_activeGun != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun = weaponView;

            _rigController.SetRigWeightUnarmed();
            _animatorController.Unarmed();
            yield return new WaitForSeconds(0.3f);

            yield return null;
        }

        //спавнит оружие из слота1 или слота2, если они оба пусты то устанавливает безоружный режим
        private void DefaultCurrentWeapon()
        {
            if (initializeOnStart)
            {
                if (AllWeaponViewModels.Count == 0)
                {
                    Debug.LogError($"WeaponViewModels not found");
                    return;
                }

                if (WeaponSlot1 != null)
                {
                    if (WeaponSlot2 == null)
                    {
                        WeaponSlot2 = _unarmedView;
                    }

                    SetBehaviorByWeaponType(WeaponSlot1.WeaponType);
                    _activeGun = WeaponSlot1;
                    _activeGun.Spawn();
                    return;
                }

                if (WeaponSlot2 != null)
                {
                    WeaponSlot1 = _unarmedView;
                    SetBehaviorByWeaponType(WeaponSlot2.WeaponType);
                    _activeGun = WeaponSlot2;
                    _activeGun.Spawn();
                    return;
                }

                WeaponSlot1 = _unarmedView;
                WeaponSlot2 = _unarmedView;
                SetBehaviorByWeaponType(WeaponType.Unarmed);
                _activeGun = _unarmedView;
            }
        }

        private void SetBehaviorByWeaponType(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.Unarmed:
                    _rigController.SetRigWeightUnarmed();
                    _animatorController.Unarmed();

                    break;
                case WeaponType.Rifle:

                    _rigController.SetRigWeightGetRifle();
                    _animatorController.GetRifle();

                    break;
                case WeaponType.Pistol:

                    _rigController.SetRigWeightGetPistol();
                    _animatorController.GetPistol();

                    break;
            }
        }

        private bool ShouldManualReload()
        {
            return !_reloading && _activeGun.CanReload();
        }

        private bool ShouldAutoReload()
        {
            return !_reloading
                   && autoReload
                   && _activeGun.CurrentAmmo == 0
                   && _activeGun.CanReload();
        }

        private IEnumerator SetRigHand()
        {
            yield return new WaitForEndOfFrame();
            _rigController.SetRigLayerLeftHandIK(1, _activeGun.WeaponType);
        }
    }
}