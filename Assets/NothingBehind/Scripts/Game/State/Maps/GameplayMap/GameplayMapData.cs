using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.State.Entities.Characters;
using NothingBehind.Scripts.Game.State.Entities.Storages;
using NothingBehind.Scripts.Game.State.Maps.EnemySpawns;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;

namespace NothingBehind.Scripts.Game.State.Maps.GameplayMap
{
    [Serializable]
    public class GameplayMapData
    {
        public MapId Id;
        public string SceneName;
        public List<CharacterData> Characters;
        public List<StorageData> Storages;
        public List<MapTransferData> MapTransfers;
        public List<EnemySpawnData> EnemySpawns;
    }
}