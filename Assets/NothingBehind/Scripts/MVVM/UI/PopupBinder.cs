using UnityEngine;
using UnityEngine.UI;

namespace NothingBehind.Scripts.MVVM.UI
{
    public class PopupBinder<T> : WindowBinder<T> where T : WindowViewModel
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnCloseAlt;

        protected virtual void OnEnable()
        {
            _btnClose?.onClick.AddListener(OnCLoseButtonClick);
            _btnCloseAlt?.onClick.AddListener(OnCLoseButtonClick);
        }

        protected virtual void OnDestroy()
        {
            _btnClose?.onClick.RemoveListener(OnCLoseButtonClick);
            _btnCloseAlt?.onClick.RemoveListener(OnCLoseButtonClick);
        }

        private void OnCLoseButtonClick()
        {
            ViewModel.RequestClose();
        }
    }
}