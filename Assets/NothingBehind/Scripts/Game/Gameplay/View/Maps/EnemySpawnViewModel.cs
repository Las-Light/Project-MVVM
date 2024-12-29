using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.View.Maps
{
    public class EnemySpawnViewModel
    {
        public string Id;
        public List<CharacterInitialStateSettings> Characters;
        public Vector3 Position;
        
        private readonly CharactersService _charactersService;
        
        public EnemySpawnViewModel(EnemySpawnData enemySpawnData, CharactersService charactersService)
        {
            _charactersService = charactersService;
            Id = enemySpawnData.Id;
            Characters = enemySpawnData.Characters;
            Position = enemySpawnData.Position;
        }

        public void SpawnEnemies()
        {
            foreach (var character in Characters)
            {
                _charactersService.CreateCharacter(character.TypeId, character.LevelSettings.Level, character.Position);
            }
        }
    }
}