using NothingBehind.Scripts.Utils;

namespace NothingBehind.Scripts.Game.State.Commands
{
    public interface ICommandProcessor
    {
        void RegisterHandler<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand;
        CommandResult Process<TCommand>(TCommand command) where TCommand : ICommand;
    }
}