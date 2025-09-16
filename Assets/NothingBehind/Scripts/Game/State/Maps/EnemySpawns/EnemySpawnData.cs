using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps.EnemySpawns
{
    [Serializable]
    public class EnemySpawnData
    {
        public string Id;
        public List<EntityInitialStateSettings> Characters;
        public Vector3 Position;
        public bool IsTriggered;

        public EnemySpawnData(
            string spawnId,
            List<EntityInitialStateSettings> characters,
            Vector3 position, 
            bool isTriggered)
        {
            Id = spawnId;
            Characters = characters;
            Position = position;
            IsTriggered = isTriggered;
        }
    }
}