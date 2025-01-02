using UnityEngine;

namespace NothingBehind.Scripts.MVVM.UI
{
    public class WindowBinder<T> : MonoBehaviour, IWindowBinder where T : WindowViewModel
    {
        protected T ViewModel;

        public void Bind(WindowViewModel viewModel)
        {
            ViewModel = (T)viewModel;

            OnBind(ViewModel);
        }

        public virtual void Close()
        {
            // Здесь сначала будем уничтожать, а потом можно делать анимацию закрытие
            Destroy(gameObject);
        }

        protected virtual void OnBind(T viewModel) { }
    }
}