using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public class EnemySpawnProxy
    {
        public string Id => Origin.Id;
        public List<CharacterInitialStateSettings> Characters { get; } = new();
        public readonly Vector3 Position;
        public readonly ReactiveProperty<bool> Triggered;
        public readonly EnemySpawnData Origin;

        public EnemySpawnProxy(EnemySpawnData spawnData)
        {
            Origin = spawnData;
            spawnData.Characters.ForEach(Characters.Add);
            Position = spawnData.Position;
            Triggered = new ReactiveProperty<bool>(spawnData.IsTriggered);

            Triggered.Subscribe(isTriggered => spawnData.IsTriggered = isTriggered);
        }
    }
}