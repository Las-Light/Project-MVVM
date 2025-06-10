using System;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.State.Items.EquippedItems.MagazinesItems;
using NothingBehind.Scripts.MVVM.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.BattleGameplay.MVVM.UI.ScreenGameplay
{
    public class ScreenGameplayView : WindowView<ScreenGameplayViewModel>
    {
        [SerializeField] private Button _btnPopupA;
        [SerializeField] private Button _btnPopupB;
        [SerializeField] private Button _btnGoToMenu;
        [SerializeField] private Button _btnInventory;
        [SerializeField] private TMP_Text _ammoCount;
        [SerializeField] private TMP_Text _magazinesCount;
        [SerializeField] private TMP_Text _allAmmoCount;

        private ReadOnlyReactiveProperty<int> CurrentAmmo;
        private IDisposable _subscriptionCurrentAmmo;
        private ReadOnlyReactiveProperty<WeaponViewModel> CurrentWeapon;

        private ReadOnlyReactiveProperty<int> _appropriateMagazinesCount;
        private ReadOnlyReactiveProperty<int> _appropriateAmmoCount;
        private IDisposable _subscriptionCountMagazines;
        private IDisposable _subscriptionCountAmmo;
        private ReactiveProperty<MagazinesItem> Magazines { get; set; }
        private IDisposable _subscriptionMagazines;

        private CompositeDisposable _disposables = new();


        protected override void OnBind(ScreenGameplayViewModel viewModel)
        {
            CurrentWeapon = viewModel.ArsenalViewModel.CurrentWeapon;
            _disposables.Add(CurrentWeapon.Subscribe(activeGun =>
            {
                if (_subscriptionCurrentAmmo != null)
                {
                    _subscriptionCurrentAmmo.Dispose();
                }

                if (_subscriptionCountMagazines != null)
                {
                    _subscriptionCountMagazines.Dispose();
                }

                if (_subscriptionMagazines != null)
                {
                    _subscriptionMagazines.Dispose();
                }

                if (_subscriptionCountAmmo != null)
                {
                    _subscriptionCountAmmo.Dispose();
                }

                _appropriateMagazinesCount = activeGun.AppropriateMagazinesCount;
                _appropriateAmmoCount = activeGun.AppropriateAmmoCount;
                Magazines = activeGun.FeedSystem.MagazinesItem;
                _subscriptionMagazines = Magazines.Subscribe(currentMagazines =>
                {
                    CurrentAmmo = currentMagazines.Magazines.CurrentAmmo;
                    _subscriptionCurrentAmmo = CurrentAmmo.Subscribe(currentAmmo =>
                    {
                        _ammoCount.text = $"Current Ammo - {currentAmmo}";
                    }).AddTo(_disposables);
                });

                _subscriptionCountAmmo = _appropriateAmmoCount.Subscribe(ammo =>
                {
                    _allAmmoCount.text = $"All Ammo - {ammo}";
                }).AddTo(_disposables);
                _subscriptionCountMagazines = _appropriateMagazinesCount.Subscribe(value =>
                {
                    _magazinesCount.text = $"Magazines - {value}";
                }).AddTo(_disposables);
            }));
        }

        private void OnEnable()
        {
            _btnPopupA.onClick.AddListener(OnPopupAButtonClicked);
            _btnPopupB.onClick.AddListener(OnPopupBButtonClicked);
            _btnGoToMenu.onClick.AddListener(OnGoToMainMenuButtonClicked);
            _btnInventory.onClick.AddListener(OnInventoryButtonClicked);
        }

        private void OnDisable()
        {
            _btnPopupA.onClick.RemoveListener(OnPopupAButtonClicked);
            _btnPopupB.onClick.RemoveListener(OnPopupBButtonClicked);
            _btnGoToMenu.onClick.RemoveListener(OnGoToMainMenuButtonClicked);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void OnInventoryButtonClicked()
        {
            ViewModel.RequestOpenInventory(0);
        }

        private void OnPopupAButtonClicked()
        {
            ViewModel.RequestOpenPopupA();
        }

        private void OnPopupBButtonClicked()
        {
            //ViewModel.RequestOpenPopupB();
            gameObject.SetActive(false);
        }

        private void OnGoToMainMenuButtonClicked()
        {
            ViewModel.RequestGoToMainMenu();
        }
    }
}