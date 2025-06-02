using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class GameplayMapTransferService
    {
        private readonly ObservableList<GameplayMapTransferViewModel> _mapTransfers = new();
        private readonly Dictionary<MapId, GameplayMapTransferViewModel> _mapTransfersMap = new();

        public IObservableCollection<GameplayMapTransferViewModel> MapTransfers => _mapTransfers;

        public GameplayMapTransferService(
            IObservableCollection<MapTransferData> mapTransfers)
        {
            InitialMapTransfers(mapTransfers);
        }

        private void InitialMapTransfers(IObservableCollection<MapTransferData> mapTransfers)
        {
            foreach (var mapTransferData in mapTransfers)
            {
                CreateMapTransferViewModel(mapTransferData);
            }
            mapTransfers.ObserveAdd().Subscribe(e => CreateMapTransferViewModel(e.Value));
            mapTransfers.ObserveRemove().Subscribe(e => RemoveMapTransferViewModel(e.Value));
        }

        private void CreateMapTransferViewModel(MapTransferData mapTransferData)
        {
            var mapTransferViewModel = new GameplayMapTransferViewModel(mapTransferData);
            _mapTransfersMap[mapTransferData.TargetMapId] = mapTransferViewModel;

            _mapTransfers.Add(mapTransferViewModel);
        }

        private void RemoveMapTransferViewModel(MapTransferData mapTransferData)
        {
            if (_mapTransfersMap.TryGetValue(mapTransferData.TargetMapId, out var mapTransferViewModel))
            {
                _mapTransfers.Remove(mapTransferViewModel);
                _mapTransfersMap.Remove(mapTransferData.TargetMapId);
            }
        }
    }
}