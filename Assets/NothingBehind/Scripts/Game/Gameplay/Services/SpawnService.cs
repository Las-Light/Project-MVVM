using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawn;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class SpawnService
    {
        public readonly ObservableList<EnemySpawnViewModel> EnemySpawns = new();
        private readonly Dictionary<string, EnemySpawnViewModel> _enemySpawns = new();

        private readonly Map _loadingMap;
        private readonly CharactersService _charactersService;
        private readonly ICommandProcessor _cmd;

        public SpawnService(Map loadingMap, CharactersService charactersService, ICommandProcessor cmd)
        {
            _loadingMap = loadingMap;
            _charactersService = charactersService;
            _cmd = cmd;

            InitialEnemySpawn();
        }

        public bool TriggeredEnemySpawn(string id)
        {
            var command = new CmdTriggeredEnemySpawn(id);

            return _cmd.Process(command);
        }

        private void InitialEnemySpawn()
        {
            var enemySpawns = _loadingMap.EnemySpawns;
            enemySpawns.ForEach(CreateEnemySpawnViewModel);
            enemySpawns.ObserveAdd().Subscribe(e => CreateEnemySpawnViewModel(e.Value));
            enemySpawns.ObserveRemove().Subscribe(e => RemoveEnemySpawn(e.Value));
        }

        private void CreateEnemySpawnViewModel(EnemySpawnProxy enemySpawnProxy)
        {
            var viewModel = new EnemySpawnViewModel(enemySpawnProxy, _charactersService, this);
            _enemySpawns[enemySpawnProxy.Id] = viewModel;

            EnemySpawns.Add(viewModel);
        }

        private void RemoveEnemySpawn(EnemySpawnProxy enemySpawnData)
        {
            if (_enemySpawns.TryGetValue(enemySpawnData.Id, out var viewModel))
            {
                EnemySpawns.Remove(viewModel);
                _enemySpawns.Remove(enemySpawnData.Id);
            }
        }
    }
}