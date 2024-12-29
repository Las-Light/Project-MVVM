using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Characters;

namespace NothingBehind.Scripts.Game.State.Maps
{
    [Serializable]
    public class MapState
    {
        public MapId Id;
        public string SceneName;
        public List<CharacterEntity> Characters;
        public List<MapTransferData> MapTransfers;
        public List<EnemySpawnData> EnemySpawns;
    }
}