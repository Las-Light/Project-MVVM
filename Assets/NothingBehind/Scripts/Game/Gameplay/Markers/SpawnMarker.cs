using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.Settings.Gameplay.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Markers
{
    public class SpawnMarker: MonoBehaviour
    {
        public string Id;
        public EnemySpawnData EnemySpawn;
    }
}