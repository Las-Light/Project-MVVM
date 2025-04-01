using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Maps
{
    public class EnemySpawnViewModel
    {
        public readonly string Id;
        public readonly List<CharacterInitialStateSettings> Characters;
        public readonly Vector3 Position;
        public readonly ReactiveProperty<bool> Triggered;
        
        private readonly CharactersService _charactersService;
        private readonly SpawnService _spawnService;

        public EnemySpawnViewModel(EnemySpawn enemySpawn, CharactersService charactersService, SpawnService spawnService)
        {
            _charactersService = charactersService;
            _spawnService = spawnService;
            
            Id = enemySpawn.Id;
            Characters = enemySpawn.Characters;
            Position = enemySpawn.Position;
            Triggered = enemySpawn.Triggered;
        }

        public void SpawnEnemies()
        {
            foreach (var character in Characters)
            {
                _charactersService.CreateCharacter(character.EntityType, character.LevelSettings.Level, character.Position);
            }
        }

        public CommandResult TriggeredEnemySpawn()
        {
            Triggered.Value = true;
            return _spawnService.TriggeredEnemySpawn(Id);
        }
    }
}