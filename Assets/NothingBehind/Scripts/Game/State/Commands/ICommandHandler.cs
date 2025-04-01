using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.State.Commands
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        CommandResult Handle(TCommand command);
    }
}