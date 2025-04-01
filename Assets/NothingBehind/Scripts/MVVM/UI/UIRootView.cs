using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.MVVM.UI
{
    public class UIRootView : MonoBehaviour
    {
        [SerializeField] private WindowsContainer _windowsContainer;
        private readonly CompositeDisposable _subscription = new();
        
        public void Bind(UIRootViewModel viewModel)
        {
            _subscription.Add(viewModel.OpenedScreen.Subscribe(newScreenViewModel =>
            {
                _windowsContainer.OpenScreen(newScreenViewModel);
            }));

            foreach (var openedPopup in viewModel.OpenedPopups)
            {
                _windowsContainer.OpenPopup(openedPopup);
            }
            
            _subscription.Add(viewModel.OpenedPopups.ObserveAdd().
                Subscribe(e =>
            {
                _windowsContainer.OpenPopup(e.Value);
            }));
            
            _subscription.Add(viewModel.OpenedPopups.ObserveRemove().
                Subscribe(e =>
            {
                _windowsContainer.ClosePopup(e.Value);
            }));
            
            OnBind(viewModel);
        }
        
        protected virtual void OnBind(UIRootViewModel viewModel){}

        private void OnDestroy()
        {
            _subscription.Dispose();
        }
    }
}