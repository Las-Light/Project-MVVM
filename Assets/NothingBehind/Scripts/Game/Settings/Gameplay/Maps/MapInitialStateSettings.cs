using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawn;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [Serializable]
    public class MapInitialStateSettings
    {
        public Vector3 PlayerInitialPosition;
        public List<CharacterInitialStateSettings> Characters;
        public List<MapTransferData> MapTransfers;
        public List<EnemySpawnData> EnemySpawns;

        public MapInitialStateSettings(
            Vector3 playerInitialPosition,
            List<CharacterInitialStateSettings> characters, 
            List<MapTransferData> mapTransfers, 
            List<EnemySpawnData> enemySpawns)
        {
            PlayerInitialPosition = playerInitialPosition;
            Characters = characters;
            MapTransfers = mapTransfers;
            EnemySpawns = enemySpawns;
        }
    }
}