using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [Serializable]
    public class MapInitialStateSettings
    {
        public MapType MapType;
        public Vector3 PlayerInitialPosition;
        public List<EntityInitialStateSettings> Entities;
        public List<MapTransferData> MapTransfers;
        public List<EnemySpawnData> EnemySpawns;

        public MapInitialStateSettings(
            Vector3 playerInitialPosition,
            List<EntityInitialStateSettings> entities, 
            List<MapTransferData> mapTransfers, 
            List<EnemySpawnData> enemySpawns)
        {
            PlayerInitialPosition = playerInitialPosition;
            Entities = entities;
            MapTransfers = mapTransfers;
            EnemySpawns = enemySpawns;
        }
    }
}