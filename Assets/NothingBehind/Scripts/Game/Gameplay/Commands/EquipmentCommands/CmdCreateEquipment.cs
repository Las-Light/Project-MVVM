using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Entities;

namespace NothingBehind.Scripts.Game.Gameplay.Commands.EquipmentCommands
{
    public class CmdCreateEquipment : ICommand
    {
        public readonly int OwnerId;
        public readonly EntityType OwnerType;

        public CmdCreateEquipment(int ownerId, EntityType ownerType)
        {
            OwnerId = ownerId;
            OwnerType = ownerType;
        }
    }
}