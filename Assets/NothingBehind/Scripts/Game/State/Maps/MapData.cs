using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;

namespace NothingBehind.Scripts.Game.State.Maps
{
    public class MapData
    {
        public MapType MapType { get; set; }
        public MapId Id { get; set; }
        public string SceneName { get; set; }
        public List<EntityData> Entities { get; set; }
        public List<MapTransferData> MapTransfers { get; set; }
        public List<EnemySpawnData> EnemySpawns { get; set; }
    }
}