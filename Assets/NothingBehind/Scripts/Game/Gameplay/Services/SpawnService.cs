using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class SpawnService
    {
        private readonly ObservableList<EnemySpawnViewModel> _enemySpawns = new();
        private readonly Dictionary<string, EnemySpawnViewModel> _enemySpawnsMap = new();

        private readonly CharactersService _charactersService;
        private readonly ICommandProcessor _cmd;

        public IObservableCollection<EnemySpawnViewModel> EnemySpawns => _enemySpawns;

        public SpawnService(IObservableCollection<EnemySpawn> enemySpawns, 
            CharactersService charactersService, ICommandProcessor cmd)
        {
            _charactersService = charactersService;
            _cmd = cmd;

            InitialEnemySpawn(enemySpawns);
        }

        public bool TriggeredEnemySpawn(string id)
        {
            var command = new CmdTriggeredEnemySpawn(id);

            return _cmd.Process(command);
        }

        private void InitialEnemySpawn(IObservableCollection<EnemySpawn> enemySpawns)
        {
            foreach (var enemySpawn in enemySpawns)
            {
                CreateEnemySpawnViewModel(enemySpawn);
            }
            enemySpawns.ObserveAdd().Subscribe(e => CreateEnemySpawnViewModel(e.Value));
            enemySpawns.ObserveRemove().Subscribe(e => RemoveEnemySpawn(e.Value));
        }

        private void CreateEnemySpawnViewModel(EnemySpawn enemySpawn)
        {
            var viewModel = new EnemySpawnViewModel(enemySpawn, _charactersService, this);
            _enemySpawnsMap[enemySpawn.Id] = viewModel;

            _enemySpawns.Add(viewModel);
        }

        private void RemoveEnemySpawn(EnemySpawn enemySpawnData)
        {
            if (_enemySpawnsMap.TryGetValue(enemySpawnData.Id, out var viewModel))
            {
                _enemySpawns.Remove(viewModel);
                _enemySpawnsMap.Remove(enemySpawnData.Id);
            }
        }
    }
}