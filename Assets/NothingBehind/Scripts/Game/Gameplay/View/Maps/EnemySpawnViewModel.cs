using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class EnemySpawnViewModel
    {
        public readonly string Id;
        public readonly List<CharacterInitialStateSettings> Characters;
        public readonly Vector3 Position;
        public readonly ReactiveProperty<bool> Triggered;
        
        private readonly CharactersService _charactersService;
        private readonly SpawnService _spawnService;

        public EnemySpawnViewModel(EnemySpawnProxy enemySpawnProxy, CharactersService charactersService, SpawnService spawnService)
        {
            _charactersService = charactersService;
            _spawnService = spawnService;
            
            Id = enemySpawnProxy.Id;
            Characters = enemySpawnProxy.Characters;
            Position = enemySpawnProxy.Position;
            Triggered = enemySpawnProxy.Triggered;
        }

        public void SpawnEnemies()
        {
            foreach (var character in Characters)
            {
                _charactersService.CreateCharacter(character.TypeId, character.LevelSettings.Level, character.Position);
            }
        }

        public bool TriggeredEnemySpawn()
        {
            Triggered.Value = true;
            return _spawnService.TriggeredEnemySpawn(Id);
        }
    }
}