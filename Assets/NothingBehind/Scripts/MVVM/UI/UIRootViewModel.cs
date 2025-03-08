using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.MVVM.UI
{
    public class UIRootViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<WindowViewModel> OpenedScreen => _openedScreen;
        public IObservableCollection<WindowViewModel> OpenedPopups => _openedPopups;

        private readonly ReactiveProperty<WindowViewModel> _openedScreen = new(null);
        private readonly ObservableList<WindowViewModel> _openedPopups = new();
        private readonly Dictionary<WindowViewModel, IDisposable> _popupSubscription = new();

        public void Dispose()
        {
            CloseAllPopups();
            _openedScreen?.Dispose();
        }

        public void OpenSreen(WindowViewModel screenViewModel)
        {
            _openedScreen.Value?.Dispose();
            _openedScreen.Value = screenViewModel;
        }

        public void OpenPopup(WindowViewModel popupViewModel)
        {
            if (_openedPopups.Contains(popupViewModel))
            {
                return;
            }

            var subscription = popupViewModel.CloseRequested.Subscribe(ClosePopup);
            _popupSubscription.Add(popupViewModel, subscription);
            
            _openedPopups.Add(popupViewModel);
        }

        public void ClosePopup(WindowViewModel popupViewModel)
        {
            if (_openedPopups.Contains(popupViewModel))
            {
                popupViewModel.Dispose();
                _openedPopups.Remove(popupViewModel);

                var popupSubscription = _popupSubscription[popupViewModel];
                popupSubscription?.Dispose();
                _popupSubscription.Remove(popupViewModel);
            }
        }

        public void ClosePopup(string popupId)
        {
            var openedPopupViewModel = _openedPopups.FirstOrDefault(p => p.Id == popupId);
            ClosePopup(openedPopupViewModel);
        }

        public void CloseAllPopups()
        {
            foreach (var openedPopup in _openedPopups)
            {
                ClosePopup(openedPopup);
            }
        }
    }
}