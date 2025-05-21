using System.Collections.Generic;
using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Settings.Gameplay.Characters;
using NothingBehind.Scripts.Game.State.Entities;
using NothingBehind.Scripts.Game.State.Entities.Player;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.Player
{
    public class PlayerViewModel
    {
        public int Id { get; }
        public EntityType EntityType;
        public IObservableCollection<PositionOnMap> PositionOnMaps => _positionOnMaps;
        public ReadOnlyReactiveProperty<MapId> CurrentMapId { get; }
        public ReadOnlyReactiveProperty<float> Health { get; }
        public ReadOnlyReactiveProperty<Vector3> Position { get; }
        public readonly ArsenalViewModel ArsenalViewModel;
        public readonly GameplayInputManager InputManager;
        public readonly PlayerSettings PlayerSettings;
        
        public GameObject CurrentEnemy { get; set; }
        public bool IsAim;
        public bool IsCrouch;
        public bool IsSprint;
        public bool IsCheckWall;
        public bool IsReloading;

        public List<GameObject> VisibleTargets = new List<GameObject>();
        
        private ObservableList<PositionOnMap> _positionOnMaps { get; }
        
        private readonly State.Entities.Player.Player _player;
        private readonly PlayerService _playerService;
        private PlayerView _playerView;
        private CharacterController _playerCharacterController;
        private PlayerInput _playerInput;

        public PlayerViewModel(State.Entities.Player.Player player,
            PlayerService playerService,
            GameplayInputManager inputManager,
            ArsenalViewModel arsenalViewModel, 
            PlayerSettings playerSettings)
        {
            Id = player.Id;
            EntityType = player.EntityType;
            CurrentMapId = player.CurrentMapId;
            Health = player.Health;
            _positionOnMaps = player.PositionOnMaps;
            
            _player = player;
            _playerService = playerService;
            ArsenalViewModel = arsenalViewModel;
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