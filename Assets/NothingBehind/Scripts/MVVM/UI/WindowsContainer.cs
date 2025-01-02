using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.MVVM.UI
{
    public class WindowsContainer : MonoBehaviour
    {
        [SerializeField] private Transform _screensContainer;
        [SerializeField] private Transform _popupsContainer;

        private readonly Dictionary<WindowViewModel, IWindowBinder> _openedPopupBinders = new();
        private IWindowBinder _openedScreenBinder;

        public void OpenPopup(WindowViewModel popupViewModel)
        {
            var prefabPath = GetPrefabPath(popupViewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, _popupsContainer);
            var binder = createdPopup.GetComponent<IWindowBinder>();
            
            binder.Bind(popupViewModel);
            _openedPopupBinders.Add(popupViewModel, binder);
        }

        public void ClosePopup(WindowViewModel popupViewModel)
        {
            var binder = _openedPopupBinders[popupViewModel];
            
            binder?.Close();
            _openedPopupBinders.Remove(popupViewModel);
        }

        public void OpenScreen(WindowViewModel screenViewModel)
        {
            if (screenViewModel == null)
                return;

            _openedScreenBinder?.Close();
            
            var prefabPath = GetPrefabPath(screenViewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdScreen = Instantiate(prefab, _screensContainer);
            var binder = createdScreen.GetComponent<IWindowBinder>();
            
            binder.Bind(screenViewModel);
            _openedScreenBinder = binder;
        }

        private static string GetPrefabPath(WindowViewModel viewModel)
        {
            return $"Prefabs/UI/{viewModel.Id}";
        }
    }
}