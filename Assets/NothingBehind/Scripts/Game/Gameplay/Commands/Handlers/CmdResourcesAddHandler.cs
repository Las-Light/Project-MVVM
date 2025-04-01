using System.Linq;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Root;
using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.Handlers
{
    public class CmdResourcesAddHandler : ICommandHandler<CmdResourcesAdd>
    {
        private readonly GameStateProxy _gameState;

        public CmdResourcesAddHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public CommandResult Handle(CmdResourcesAdd command)
        {
            var requiredResourceType = command.ResourceType;
            var requiredResource = _gameState.Resources.FirstOrDefault(
                r => r.ResourceType == requiredResourceType);
            
            if (requiredResource == null)
            {
                requiredResource = CreateNewResource(requiredResourceType);
            }

            requiredResource.Amount.Value += command.Amount;

            return new CommandResult( true);
        }

        private Resource CreateNewResource(ResourceType resourceType)
        {
            var newResourceData = new ResourceData()
            {
                ResourceType = resourceType,
                Amount = 0
            };
            
            var newResource = new Resource(newResourceData);
            _gameState.Resources.Add(newResource);            
            
            return newResource;
        }
    }
}