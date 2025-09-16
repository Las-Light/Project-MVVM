using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps.EnemySpawns
{
    public class EnemySpawn
    {
        public string Id => _origin.Id;
        public List<EntityInitialStateSettings> Characters { get; } = new();
        public readonly Vector3 Position;
        public readonly ReactiveProperty<bool> Triggered;
        private readonly EnemySpawnData _origin;

        public EnemySpawn(EnemySpawnData spawnData)
        {
            _origin = spawnData;
            spawnData.Characters.ForEach(Characters.Add);
            Position = spawnData.Position;
            Triggered = new ReactiveProperty<bool>(spawnData.IsTriggered);

            Triggered.Subscribe(isTriggered => spawnData.IsTriggered = isTriggered);
        }
    }
}