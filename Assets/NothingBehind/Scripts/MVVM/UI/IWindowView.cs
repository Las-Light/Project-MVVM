namespace NothingBehind.Scripts.MVVM.UI
{
    public interface IWindowView
    {
        void Bind(WindowViewModel viewModel);
        void Close();
    }
}