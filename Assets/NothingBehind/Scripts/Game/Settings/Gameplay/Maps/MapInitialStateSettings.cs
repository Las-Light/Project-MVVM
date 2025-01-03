using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Markers;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Maps;

namespace NothingBehind.Scripts.Game.Settings.Gameplay.Maps
{
    [Serializable]
    public class MapInitialStateSettings
    {
        public List<CharacterInitialStateSettings> Characters;
        public List<MapTransferData> MapTransfers;
        public List<EnemySpawnData> EnemySpawns;

        public MapInitialStateSettings(
            List<CharacterInitialStateSettings> characters, 
            List<MapTransferData> mapTransfers, 
            List<EnemySpawnData> enemySpawns)
        {
            Characters = characters;
            MapTransfers = mapTransfers;
            EnemySpawns = enemySpawns;
        }
    }
}