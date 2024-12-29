using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State.Maps;
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
        
        public SpawnService(Map loadingMap, CharactersService charactersService)
        {
            _loadingMap = loadingMap;
            _charactersService = charactersService;
            
            InitialEnemySpawn();
        }
        
        private void InitialEnemySpawn()
        {
            var enemySpawns = _loadingMap.EnemySpawns;
            enemySpawns.ForEach(CreateEnemySpawnViewModel);
            enemySpawns.ObserveAdd().Subscribe(e => CreateEnemySpawnViewModel(e.Value));
            enemySpawns.ObserveRemove().Subscribe(e => RemoveEnemySpawn(e.Value));
        }
        
        private void CreateEnemySpawnViewModel(EnemySpawnData enemySpawnData)
        {
            var viewModel = new EnemySpawnViewModel(enemySpawnData, _charactersService);
            _enemySpawns[enemySpawnData.Id] = viewModel;

            EnemySpawns.Add(viewModel);
        }

        private void RemoveEnemySpawn(EnemySpawnData enemySpawnData)
        {
            if (_enemySpawns.TryGetValue(enemySpawnData.Id, out var viewModel))
            {
                EnemySpawns.Remove(viewModel);
                _enemySpawns.Remove(enemySpawnData.Id);
            }
        }
    }
}