using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps.EnemySpawns
{
    [Serializable]
    public class EnemySpawnData
    {
        public string Id;
        public List<CharacterInitialStateSettings> Characters;
        public Vector3 Position;
        public bool IsTriggered;

        public EnemySpawnData(string spawnId, List<CharacterInitialStateSettings> characters, Vector3 position, bool isTriggered)
        {
            Id = spawnId;
            Characters = characters;
            Position = position;
            IsTriggered = isTriggered;
        }
    }
}