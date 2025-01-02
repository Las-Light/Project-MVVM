using DI.Scripts;

namespace NothingBehind.Scripts.MVVM.UI
{
    public abstract class UIManager
    {
        protected readonly DIContainer Container; // Чтобы вытаскивать барахло и собирать вьюмодели окошек

        protected UIManager(DIContainer container)
        {
            Container = container;
        }
    }
}