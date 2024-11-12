using System;
using System.Collections.Generic;

namespace NothingBehind.Scripts.Game.State.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IGameStateProvider _gameStateProvider;
        private Dictionary<Type, object> _handlesMap = new();

        public CommandProcessor(IGameStateProvider gameStateProvider)
        {
            _gameStateProvider = gameStateProvider;
        }

        public void RegisterHandler<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            _handlesMap[typeof(TCommand)] = handler;
        }

        public bool Process<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (_handlesMap.TryGetValue(typeof(TCommand), out var handler))
            {
                var typeHandler = (ICommandHandler<TCommand>)handler;
                var result = typeHandler.Handle(command);

                // сохранение состояния после выполнения команды, не всегда это нужно 
                if (result)
                {
                    _gameStateProvider.SaveGameState();
                }

                return result;
            }

            return false;
        }
    }
}