using System.Linq;
using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
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
        
        private readonly State.Entities.Player.Player _player;
        private readonly PlayerService _playerService;

        public PlayerViewModel(State.Entities.Player.Player player,
            PlayerService playerService,
            InputManager inputManager,
            PlayerSettings playerSettings)
        {
            Id = player.Id;
            EntityType = player.EntityType;
            CurrentMapId = player.CurrentMapId;
            Health = player.Health;
            _positionOnMaps = player.PositionOnMaps;
            
            _player = player;
            _playerService = playerService;
            InputManager = inputManager;
            PlayerSettings = playerSettings;

            //Инициализируем позицию игрока из данных 
            var currentPosOnMap = _positionOnMaps.FirstOrDefault(map => map.MapId == CurrentMapId.CurrentValue);
            if (currentPosOnMap != null) Position = currentPosOnMap.Position;
        }

        public void UpdatePlayerPosition(Vector3 position)
        {
            _playerService.UpdatePlayerPosOnMap(position, CurrentMapId.CurrentValue);
        }
    }
}