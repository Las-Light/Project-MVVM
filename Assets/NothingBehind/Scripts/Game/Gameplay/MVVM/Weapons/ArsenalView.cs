using System;
using System.Collections;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.Logic.Player;
using NothingBehind.Scripts.Game.Gameplay.Logic.Sound;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
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
        public WeaponView WeaponSlot1 => _weaponSlot1;
        public WeaponView WeaponSlot2 => _weaponSlot2;

        public WeaponView ActiveGun { get; private set; }

        private WeaponView _weaponSlot1;
        private WeaponView _weaponSlot2;
        private readonly ReactiveProperty<WeaponView> _activeGun = new();
        private WeaponView _unarmedView;
        private Transform _pistolParent;
        private Transform _rifleParent;
        private Transform _unarmedParent;
        private Transform _pointToCheckClip;
        private PlayerView _playerView;
        private LayerMask _obstacleMask;
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
        private AimController _aimController;
        private Transform _gunParent;

        private bool _isShootTimerStart;
        private float _shootTimer;
        private bool _reloading;
        private bool _shootEnable;

        private CompositeDisposable _disposables = new();

        public void Bind(PlayerView playerView,
            ArsenalViewModel viewModel,
            Transform pistolParent,
            Transform rifleParent,
            Transform unarmedParent)
        {
            AllWeaponViewModels = viewModel.AllWeaponViewModels;
            EquippedItems = viewModel.EquipmentItems;
            _playerView = playerView;
            _pointToCheckClip = playerView.pointToCheckClip;
            _obstacleMask = playerView.obstacleMask;

            _animatorController = GetComponentInParent<AnimatorController>();
            _rigController = GetComponentInParent<RigController>();
            _soundController = GetComponentInParent<SoundController>();
            _aimController = GetComponentInParent<AimController>();
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
                                _weaponSlot1 = weaponView;
                                break;
                            case SlotType.Weapon2:
                                _weaponSlot2 = weaponView;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"WeaponView not found for weapon with Id {kvp.Value.Id}");
                    }
                }
            }

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
                                if (_activeGun.Value == _weaponSlot1) WeaponSwitch(weaponView);
                                _weaponSlot1 = weaponView;
                                break;
                            case SlotType.Weapon2:
                                if (_activeGun.Value == _weaponSlot2) WeaponSwitch(weaponView);
                                _weaponSlot2 = weaponView;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"WeaponView not found for weapon Id {kvp.Value.Id}");
                    }
                }
            }));
            var equipRemoveSubscribe = EquippedItems.ObserveRemove().Subscribe(e =>
            {
                var kvp = e.Value;
                switch (kvp.Key)
                {
                    case SlotType.Weapon1:
                        _weaponSlot1 = _unarmedView;
                        break;
                    case SlotType.Weapon2:
                        _weaponSlot2 = _unarmedView;
                        break;
                }
            }).AddTo(_disposables);

            _disposables.Add(AllWeaponViewModels.ObserveAdd().Subscribe(e =>
            {
                var addedWeapon = e.Value;
                SetupWeapon(addedWeapon);
            }));
            _disposables.Add(AllWeaponViewModels.ObserveRemove().Subscribe(e =>
            {
                var removedWeapon = e.Value;
                if (_weaponViewMap.TryGetValue(removedWeapon.Id, out var weaponView))
                {
                    if (_activeGun.Value == weaponView)
                    {
                        // в случае удаления оружия которое сейчас активно, переход в безоружное состояние происходит без анимации 
                        GoStateUnarmedWithoutDelay(_unarmedView);
                        DestroyWeaponView(removedWeapon.Id);
                    }
                }
            }));

            _disposables.Add(_activeGun.Skip(1).Subscribe(value =>
            {
                ActiveGun = value;
                viewModel.ActiveGunId.OnNext(value.Id);
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
                _activeGun.Value.EndReload();
                _reloading = false;
                StartCoroutine(SetRigHand());
            }
        }

        public void EndReload()
        {
            _activeGun.Value.EndReload();
            _reloading = false;
            StartCoroutine(SetRigHand());
        }


        //метод который стреляет при нажатии на кнопку "Стрелять"
        public bool Shoot()
        {
            if (_shootEnable)
            {
                if (ShouldAutoReload())
                {
                    Reload();
                    return false;
                }

                _activeGun.Value.Tick(!_reloading && _activeGun != null);
                if (_activeGun.Value.WeaponType == WeaponType.Rifle)
                {
                    _animatorController.RifleShootRecoil();
                }

                if (_activeGun.Value.WeaponType == WeaponType.Pistol)
                {
                    _animatorController.PistolShootRecoil();
                }

                //_soundController.MakeSoundSelf(ActiveGun.GetRaycastOrigin(), SoundType.Shoot);

                return true;
            }

            return false;
        }

        public void WeaponSwitch(WeaponView targetWeapon)
        {
            if (_activeGun.Value == targetWeapon)
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

        //проверяет лучом есть ли перед игроком препятствие, если есть то поднимает оружие и не дает стрелять
        //точка откуда выходит луч находится на уровне шеи, задается через инспектор (подумать как можно заменить без инспектора)
        public void ClipPrevention()
        {
            Vector3 aimDirection = (_aimController.AimPoint.position - _pointToCheckClip.position).normalized;
            if (Physics.Raycast(
                    _pointToCheckClip.position,
                    aimDirection,
                    _activeGun.Value.CheckDistanceToWall, _obstacleMask) &&
                _activeGun.Value.WeaponType != WeaponType.Unarmed)
            {
                _playerView.IsCheckWall = true;
                if (_playerView.IsAim)
                {
                    if (_activeGun.Value.WeaponType == WeaponType.Pistol)
                        _rigController.AimPistolRig(false);
                    else if (_activeGun.Value.WeaponType == WeaponType.Rifle)
                    {
                        _rigController.AimRifleRig(false);
                        _rigController.ClipWallRig(_playerView.IsCheckWall);
                    }
                }

                _shootEnable = false;
            }
            else
            {
                _shootEnable = true;
                _playerView.IsCheckWall = false;
                if (_playerView.IsAim)
                {
                    if (_activeGun.Value.WeaponType == WeaponType.Pistol)
                        _rigController.AimPistolRig(true);

                    if (_activeGun.Value.WeaponType == WeaponType.Rifle)
                    {
                        _rigController.AimRifleRig(true);
                        _rigController.ClipWallRig(_playerView.IsCheckWall);
                    }
                }
            }
        }

        private void WeaponSwitchRifle(WeaponView weaponView)
        {
            if (_activeGun.Value.WeaponType == WeaponType.Pistol)
            {
                _animatorController.RigPutPistol();
            }

            if (_activeGun.Value.WeaponType == WeaponType.Rifle && _activeGun.Value != weaponView)
            {
                _animatorController.RigPutRifle();
            }

            StartCoroutine(GoStateGetRifle(weaponView));
        }


        private void WeaponSwitchPistol(WeaponView weaponView)
        {
            if (_activeGun.Value.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }

            if (_activeGun.Value.WeaponType == WeaponType.Pistol && _activeGun.Value != weaponView)
            {
                _animatorController.RigPutPistol();
            }

            if (_activeGun.Value.WeaponType != WeaponType.Pistol)
            {
                StartCoroutine(GoStateGetPistol(weaponView));
            }
        }


        private void WeaponSwitchUnarmed(WeaponView weaponView)
        {
            if (_activeGun.Value.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }
            else if (_activeGun.Value.WeaponType == WeaponType.Pistol)
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
                if (weaponView == _activeGun.Value)
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
                _activeGun.Value.Despawn();
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
                _activeGun.Value.StartReloading();
                _animatorController.Reload();
                _reloading = true;

                _rigController.SetRigLayerLeftHandIK(0, _activeGun.Value.WeaponType);
            }
        }

        //корутина нужна для завершения анимации смены оружия
        private IEnumerator GoStateGetPistol(WeaponView weaponView)
        {
            if (_activeGun.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun.Value = weaponView;
            _activeGun.Value.Spawn();

            _animatorController.RigGetPistol();
            _rigController.SetRigWeightGetPistol();
            _rigController.SetRigLayerLeftHandIK(0, WeaponType.Pistol);
            _animatorController.GetPistol();

            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateGetRifle(WeaponView weaponView)
        {
            if (_activeGun.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun.Value = weaponView;
            _activeGun.Value.Spawn();

            _animatorController.RigGetRifle();
            _rigController.SetRigWeightGetRifle();
            _animatorController.GetRifle();
            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateUnarmed(WeaponView weaponView)
        {
            yield return new WaitForSeconds(0.3f);
            if (_activeGun.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun.Value = weaponView;

            _rigController.SetRigWeightUnarmed();
            _animatorController.Unarmed();
            yield return new WaitForSeconds(0.3f);

            yield return null;
        }

        private void GoStateUnarmedWithoutDelay(WeaponView weaponView)
        {
            if (_activeGun.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _activeGun.Value = weaponView;

            _rigController.SetRigWeightUnarmed();
            _animatorController.Unarmed();
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

                if (_weaponSlot1 != null)
                {
                    if (_weaponSlot2 == null)
                    {
                        _weaponSlot2 = _unarmedView;
                    }

                    SetBehaviorByWeaponType(_weaponSlot1.WeaponType);
                    _activeGun.Value = _weaponSlot1;
                    _activeGun.Value.Spawn();
                    return;
                }

                if (_weaponSlot2 != null)
                {
                    _weaponSlot1 = _unarmedView;
                    SetBehaviorByWeaponType(_weaponSlot2.WeaponType);
                    _activeGun.Value = _weaponSlot2;
                    _activeGun.Value.Spawn();
                    return;
                }

                _weaponSlot1 = _unarmedView;
                _weaponSlot2 = _unarmedView;
                SetBehaviorByWeaponType(WeaponType.Unarmed);
                _activeGun.Value = _unarmedView;
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
                    _rigController.SetRigLayerLeftHandIK(0, WeaponType.Pistol);
                    _animatorController.GetPistol();

                    break;
            }
        }

        private bool ShouldManualReload()
        {
            return !_reloading
                   && _activeGun.Value.CanReload()
                   && _activeGun.Value.CurrentAmmo.Value != _activeGun.Value.ClipSize;
        }

        private bool ShouldAutoReload()
        {
            return !_reloading
                   && autoReload
                   && _activeGun.Value.CurrentAmmo.Value == 0
                   && _activeGun.Value.CanReload();
        }

        private IEnumerator SetRigHand()
        {
            yield return new WaitForEndOfFrame();
            _rigController.SetRigLayerLeftHandIK(1, _activeGun.Value.WeaponType);
        }
    }
}