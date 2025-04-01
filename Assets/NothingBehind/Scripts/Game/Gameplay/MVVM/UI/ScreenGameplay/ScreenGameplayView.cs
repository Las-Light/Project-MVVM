using NothingBehind.Scripts.MVVM.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.UI.ScreenGameplay
{
    public class ScreenGameplayView : WindowView<ScreenGameplayViewModel>
    {
        [SerializeField] private Button _btnPopupA;
        [SerializeField] private Button _btnPopupB;
        [SerializeField] private Button _btnGoToMenu;
        [SerializeField] private Button _btnInventory;

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