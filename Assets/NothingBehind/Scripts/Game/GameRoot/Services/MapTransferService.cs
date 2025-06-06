using System.Collections.Generic;
using NothingBehind.Scripts.Game.GameRoot.MVVM.Transfers;
using NothingBehind.Scripts.Game.State.Maps;
using NothingBehind.Scripts.Game.State.Maps.MapTransfer;
using ObservableCollections;
using R3;

namespace NothingBehind.Scripts.Game.GameRoot.Services
{
    public class MapTransferService
    {
        private readonly ObservableList<GameplayMapTransferViewModel> _allMapTransferViewModels = new();

        private readonly Dictionary<MapId, ObservableList<GameplayMapTransferViewModel>> _mapTransfersViewModelMap = new();

        private readonly Dictionary<MapTransferData, GameplayMapTransferViewModel> _dataViewModelMaps = new();

        public ObservableList<GameplayMapTransferViewModel> AllMapTransferViewModels =>
            _allMapTransferViewModels;

        public Dictionary<MapId, ObservableList<GameplayMapTransferViewModel>> AllTransfersMaps =>
            _mapTransfersViewModelMap;

        public MapTransferService(
            Dictionary<MapId, ObservableList<MapTransferData>> mapsTransfers)
        {
            foreach (var kvp in mapsTransfers)
            {
                InitialMapTransfers(kvp);
            }
        }

        private void InitialMapTransfers(KeyValuePair<MapId, ObservableList<MapTransferData>> kvp)
        {
            _mapTransfersViewModelMap.Add(kvp.Key, new ObservableList<GameplayMapTransferViewModel>());
            foreach (var mapTransferData in kvp.Value)
            {
                CreateMapTransferViewModel(kvp.Key, mapTransferData);
            }

            kvp.Value.ObserveAdd().Subscribe(e => CreateMapTransferViewModel(kvp.Key, e.Value));
            kvp.Value.ObserveRemove().Subscribe(e => RemoveMapTransferViewModel(kvp.Key, e.Value));
        }

        private void CreateMapTransferViewModel(MapId mapId, MapTransferData mapTransferData)
        {
            var mapTransferViewModel = new GameplayMapTransferViewModel(mapTransferData);
            _mapTransfersViewModelMap[mapId].Add(mapTransferViewModel);
            _dataViewModelMaps[mapTransferData] = mapTransferViewModel;

            _allMapTransferViewModels.Add(mapTransferViewModel);
        }

        private void RemoveMapTransferViewModel(MapId mapId, MapTransferData mapTransferData)
        {
            if (_mapTransfersViewModelMap.TryGetValue(mapId, out var mapTransferViewModels))
            {
                if (_dataViewModelMaps.TryGetValue(mapTransferData, out var viewModel))
                {
                    _allMapTransferViewModels.Remove(viewModel);
                    mapTransferViewModels.Remove(viewModel);
                    _dataViewModelMaps.Remove(mapTransferData);
                }
            }
        }
    }
}