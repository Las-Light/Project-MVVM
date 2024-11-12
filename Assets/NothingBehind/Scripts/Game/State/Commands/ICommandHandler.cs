namespace NothingBehind.Scripts.Game.State.Commands
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        bool Handle(TCommand command);
    }
}