using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.GameResources;

namespace NothingBehind.Scripts.Game.GameRoot.Commands.ResourcesCommands
{
    public class CmdResourcesSpend : ICommand
    {
        public readonly ResourceType ResourceType;
        public readonly int Amount;

        public CmdResourcesSpend(ResourceType resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
        }

    }
}