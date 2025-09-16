using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay.Entities.Player;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.Player
{
    public class PlayerViewModel
    {
        public int Id { get; }
        public EntityType EntityType;
        public IObservableCollection<PositionOnMap> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<MapId> CurrentMapId { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        public readonly InputManager InputManager;
        public readonly PlayerSettings PlayerSettings;

        private ObservableList<PositionOnMap> _positionOnMaps { get; }

        private readonly PlayerEntity _playerEntity;
        private readonly PlayerService _playerService;

        public PlayerViewModel(PlayerEntity playerEntity,
            PlayerService playerService,
            InputManager inputManager,
            PlayerSettings playerSettings)
        {
            Id = playerEntity.UniqueId;
            EntityType = playerEntity.EntityType;
            CurrentMapId = playerEntity.CurrentMapId;
            Health = playerEntity.Health;
            Position = playerEntity.Position;
            _positionOnMaps = playerEntity.PositionOnMaps;

            _playerEntity = playerEntity;
            _playerService = playerService;
            InputManager = inputManager;
            PlayerSettings = playerSettings;

            //Инициализируем позицию игрока из данных 
            // var currentPosOnMap = _positionOnMaps.FirstOrDefault(map => map.MapId == CurrentMapId.CurrentValue);
            // if (currentPosOnMap != null) Position = currentPosOnMap.Position;
        }

        public void UpdatePlayerPosition(Vector3 position)
        {
            _playerService.UpdatePlayerPosOnMap(position, CurrentMapId.CurrentValue);
            _playerEntity.Position.OnNext(position);
        }
    }
}