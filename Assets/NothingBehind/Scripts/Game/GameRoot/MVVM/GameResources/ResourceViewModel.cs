using NothingBehind.Scripts.Game.State.GameResources;
using R3;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.GameResources
{
    public class ResourceViewModel
    {
        public readonly ResourceType ResourceType;
        public readonly ReadOnlyReactiveProperty<int> Amount;

        public ResourceViewModel(Resource resource)
        {
            ResourceType = resource.ResourceType;
            Amount = resource.Amount;
        }
    }
}