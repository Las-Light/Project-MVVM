using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.State.Maps.GlobalMaps
{
    public class GlobalMap
    {
        public MapId MapId => Origin.MapId;
        public string SceneName => Origin.SceneName;
        public ObservableList<MapTransferData> MapTransfers { get; } = new();
        public GlobalMapData Origin { get; }

        public GlobalMap(GlobalMapData data)
        {
            Origin = data;
            
            data.MapTransfers.ForEach(transferData => MapTransfers.Add(transferData));

            MapTransfers.ObserveAdd().Subscribe(e =>
            {
                var addedMapTransfer = e.Value;
                data.MapTransfers.Add(addedMapTransfer);
            });

            MapTransfers.ObserveRemove().Subscribe(e =>
            {
                var removedMapTransfer = e.Value;
                data.MapTransfers.Remove(removedMapTransfer);
            });
        }
    }
}