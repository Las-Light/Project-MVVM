using System;
using System.Collections;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Sound;
using NothingBehind.Scripts.Game.State.Equipments;
using NothingBehind.Scripts.Game.State.Items;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons
{
    public class ArsenalView : MonoBehaviour
    {
        [SerializeField] private GameObject _weaponViewPrefab;
        public IReadOnlyObservableList<WeaponViewModel> AllWeaponViewModels;
        public IReadOnlyObservableDictionary<SlotType, Item> EquippedItems;
        public bool IsReloading => _reloading;
        public WeaponView WeaponSlot1 => _weaponSlot1;
        public WeaponView WeaponSlot2 => _weaponSlot2;

        public WeaponView CurrentWeapon { get; private set; }
        public ReactiveProperty<SlotType> CurrentWeaponSlot { get; } = new();

        private readonly ReactiveProperty<WeaponView> _currentWeapon = new();
        private WeaponView _weaponSlot1;
        private WeaponView _weaponSlot2;
        private WeaponView _unarmedView;

        private Transform _pistolParent;
        private Transform _rifleParent;
        private Transform _unarmedParent;
        private Transform _pointToCheckClip;

        private LayerMask _obstacleMask;
        private bool autoReload = true;

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

        public void Bind(ArsenalViewModel viewModel,
            Transform pistolParent,
            Transform rifleParent,
            Transform unarmedParent,
            Transform pointToCheckClip,
            LayerMask obstacleMask)
        {
            AllWeaponViewModels = viewModel.AllWeaponViewModels;
            EquippedItems = viewModel.EquipmentItems;

            _pointToCheckClip = pointToCheckClip;
            _obstacleMask = obstacleMask;
            _animatorController = GetComponentInParent<AnimatorController>();
            _rigController = GetComponentInParent<RigController>();
            _soundController = GetComponentInParent<SoundController>();
            _aimController = GetComponentInParent<AimController>();
            _pistolParent = pistolParent;
            _rifleParent = rifleParent;
            _unarmedParent = unarmedParent;
            
            //устанавливаем _currentWeapon из ViewModel
            CurrentWeaponSlot.OnNext(viewModel.CurrentWeaponSlot.Value);

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

            EquippedItems.ObserveAdd().Subscribe(e =>
            {
                var kvp = e.Value;
                if (kvp.Key is SlotType.Weapon1 or SlotType.Weapon2)
                {
                    if (_weaponViewMap.TryGetValue(kvp.Value.Id, out var weaponView))
                    {
                        switch (kvp.Key)
                        {
                            case SlotType.Weapon1:
                                if (_currentWeapon.Value == _weaponSlot1) WeaponSwitch(weaponView);
                                _weaponSlot1 = weaponView;
                                break;
                            case SlotType.Weapon2:
                                if (_currentWeapon.Value == _weaponSlot2) WeaponSwitch(weaponView);
                                _weaponSlot2 = weaponView;
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"WeaponView not found for weapon Id {kvp.Value.Id}");
                    }
                }
            }).AddTo(_disposables);
            EquippedItems.ObserveRemove().Subscribe(e =>
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

            AllWeaponViewModels.ObserveAdd().Subscribe(e =>
            {
                var addedWeapon = e.Value;
                SetupWeapon(addedWeapon);
            }).AddTo(_disposables);
            
            AllWeaponViewModels.ObserveRemove().Subscribe(e =>
            {
                var removedWeapon = e.Value;
                if (_weaponViewMap.TryGetValue(removedWeapon.Id, out var weaponView))
                {
                    if (_currentWeapon.Value == weaponView)
                    {
                        // в случае удаления оружия которое сейчас активно, переход в безоружное состояние происходит без анимации 
                        GoStateUnarmedWithoutDelay(_unarmedView);
                        DestroyWeaponView(removedWeapon.Id);
                    }
                }
            }).AddTo(_disposables);

            _currentWeapon.Skip(1).Subscribe(value =>
            {
                CurrentWeapon = value;
                if (viewModel.WeaponViewModelMap.TryGetValue(value.Id, out var weaponViewModel))
                {
                    viewModel.CurrentWeapon.OnNext(weaponViewModel);
                }
                else
                {
                    throw new Exception($"Weapon view model with Id {value.Id} not found");
                }
            }).AddTo(_disposables);

            //Подписываемся на курентСлот и передаем его во вью-модель (важно скипать 1 событие,
            // иначе событие зарекурситься
            CurrentWeaponSlot.Skip(1).Subscribe(e =>
            {
                viewModel.CurrentWeaponSlot.OnNext(e);
            });

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
                _currentWeapon.Value.EndReload();
                _reloading = false;
                StartCoroutine(SetRigHand());
            }
        }

        public void EndReload()
        {
            _currentWeapon.Value.EndReload();
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

                _currentWeapon.Value.Tick(!_reloading && _currentWeapon != null);
                if (_currentWeapon.Value.WeaponType == WeaponType.Rifle)
                {
                    _animatorController.RifleShootRecoil();
                    _animatorController.Recoil();
                }

                if (_currentWeapon.Value.WeaponType == WeaponType.Pistol)
                {
                    _animatorController.PistolShootRecoil();
                    _animatorController.Recoil();
                }

                //_soundController.MakeSoundSelf(ActiveGun.GetRaycastOrigin(), SoundType.Shoot);

                return true;
            }

            return false;
        }

        public void WeaponSwitch(WeaponView targetWeapon)
        {
            if (_currentWeapon.Value == targetWeapon)
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
        public bool ClipPrevention(bool isAim, ref bool isCheckWall)
        {
            Vector3 aimDirection = (_aimController.AimPoint.position - _pointToCheckClip.position).normalized;
            if (Physics.Raycast(
                    _pointToCheckClip.position,
                    aimDirection,
                    _currentWeapon.Value.CheckDistanceToWall, _obstacleMask) &&
                _currentWeapon.Value.WeaponType != WeaponType.Unarmed)
            {
                isCheckWall = true;
                if (isAim)
                {
                    if (_currentWeapon.Value.WeaponType == WeaponType.Pistol)
                        _rigController.AimPistolRig(false);
                    else if (_currentWeapon.Value.WeaponType == WeaponType.Rifle)
                    {
                        _rigController.AimRifleRig(false);
                    }
                }

                _rigController.ClipWallRig(isCheckWall);

                _shootEnable = false;
            }
            else
            {
                _shootEnable = true;
                isCheckWall = false;
                if (isAim)
                {
                    if (_currentWeapon.Value.WeaponType == WeaponType.Pistol)
                        _rigController.AimPistolRig(true);

                    if (_currentWeapon.Value.WeaponType == WeaponType.Rifle)
                    {
                        _rigController.AimRifleRig(true);
                    }
                }

                _rigController.ClipWallRig(isCheckWall);
            }

            return isCheckWall;
        }

        private void WeaponSwitchRifle(WeaponView weaponView)
        {
            if (_currentWeapon.Value.WeaponType == WeaponType.Pistol)
            {
                _animatorController.RigPutPistol();
            }

            if (_currentWeapon.Value.WeaponType == WeaponType.Rifle && _currentWeapon.Value != weaponView)
            {
                _animatorController.RigPutRifle();
            }

            StartCoroutine(GoStateGetRifle(weaponView));
        }


        private void WeaponSwitchPistol(WeaponView weaponView)
        {
            if (_currentWeapon.Value.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }

            if (_currentWeapon.Value.WeaponType == WeaponType.Pistol && _currentWeapon.Value != weaponView)
            {
                _animatorController.RigPutPistol();
            }

            if (_currentWeapon.Value.WeaponType != WeaponType.Pistol)
            {
                StartCoroutine(GoStateGetPistol(weaponView));
            }
        }


        private void WeaponSwitchUnarmed(WeaponView weaponView)
        {
            if (_currentWeapon.Value.WeaponType == WeaponType.Rifle)
            {
                _animatorController.RigPutRifle();
            }
            else if (_currentWeapon.Value.WeaponType == WeaponType.Pistol)
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
                if (weaponView == _currentWeapon.Value)
                {
                    DespawnActiveGun();
                }

                _weaponViewMap.Remove(weaponId);
                Destroy(weaponView.gameObject);
            }
        }

        private void DespawnActiveGun()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.Value.Despawn();
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
                _currentWeapon.Value.StartReloading();
                _animatorController.Reload();
                _reloading = true;

                _rigController.SetRigLayerLeftHandIK(0, _currentWeapon.Value.WeaponType);
            }
        }

        //корутина нужна для завершения анимации смены оружия
        private IEnumerator GoStateGetPistol(WeaponView weaponView)
        {
            if (_currentWeapon.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _currentWeapon.Value = weaponView;
            _currentWeapon.Value.Spawn();

            _animatorController.RigGetPistol();
            _rigController.SetRigWeightGetPistol();
            _rigController.SetRigLayerLeftHandIK(0, WeaponType.Pistol);
            _animatorController.GetPistol();

            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateGetRifle(WeaponView weaponView)
        {
            if (_currentWeapon.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _currentWeapon.Value = weaponView;
            _currentWeapon.Value.Spawn();

            _animatorController.RigGetRifle();
            _rigController.SetRigWeightGetRifle();
            _animatorController.GetRifle();
            yield return new WaitWhile(() => _animatorController.State == AnimatorState.SwitchWeapon);
        }

        //корутина нужна здя завершения анимации смены оружия
        private IEnumerator GoStateUnarmed(WeaponView weaponView)
        {
            yield return new WaitForSeconds(0.3f);
            if (_currentWeapon.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _currentWeapon.Value = weaponView;

            _rigController.SetRigWeightUnarmed();
            _animatorController.Unarmed();
            yield return new WaitForSeconds(0.3f);

            yield return null;
        }

        private void GoStateUnarmedWithoutDelay(WeaponView weaponView)
        {
            if (_currentWeapon.Value != _unarmedView)
            {
                DespawnActiveGun();
            }

            _currentWeapon.Value = weaponView;

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

                if (CurrentWeaponSlot.Value == SlotType.Weapon1)
                {
                    if (_weaponSlot1 == null)
                    {
                        _weaponSlot1 = _unarmedView;
                    }
                    if (_weaponSlot2 == null)
                    {
                        _weaponSlot2 = _unarmedView;
                    }
                
                    SetBehaviorByWeaponType(_weaponSlot1.WeaponType);
                    _currentWeapon.Value = _weaponSlot1;
                }

                if (CurrentWeaponSlot.Value == SlotType.Weapon2)
                {
                    if (_weaponSlot2 == null)
                    {
                        _weaponSlot2 = _unarmedView;
                    }
                    if (_weaponSlot1 == null)
                    {
                        _weaponSlot1 = _unarmedView;
                    }
                    SetBehaviorByWeaponType(_weaponSlot2.WeaponType);
                    _currentWeapon.Value = _weaponSlot2;
                }

                if (_currentWeapon.Value != _unarmedView)
                {
                    _currentWeapon.Value.Spawn();
                }
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
                   && _currentWeapon.Value.CanReload()
                   && _currentWeapon.Value.CurrentAmmo.Value != _currentWeapon.Value.ClipSize;
        }

        private bool ShouldAutoReload()
        {
            return !_reloading
                   && autoReload
                   && _currentWeapon.Value.CurrentAmmo.Value == 0
                   && _currentWeapon.Value.CanReload();
        }

        private IEnumerator SetRigHand()
        {
            yield return new WaitForEndOfFrame();
            _rigController.SetRigLayerLeftHandIK(1, _currentWeapon.Value.WeaponType);
        }
    }
}