using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.GameResources;

namespace NothingBehind.Scripts.Game.Gameplay.Commands
{
    public class CmdResourcesAdd : ICommand
    {
        public readonly ResourceType ResourceType;
        public readonly int Amount;

        public CmdResourcesAdd(ResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }
    }
}