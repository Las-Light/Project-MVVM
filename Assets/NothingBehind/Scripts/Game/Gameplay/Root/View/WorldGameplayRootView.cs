using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Commands.StoragesCommands;
using NothingBehind.Scripts.Game.Gameplay.MVVM;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Maps;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Storages;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootView : MonoBehaviour
    {
        private readonly Dictionary<int, CharacterView> _createCharactersMap = new();
        private readonly Dictionary<int, StorageView> _createStoragesMap = new();
        private readonly Dictionary<MapId, MapTransferBinder> _createMapTransfersMap = new();
        private readonly Dictionary<string, EnemySpawnView> _createSpawns = new();
        private PlayerView _playerView;
        private CameraView _camera;
        private readonly CompositeDisposable _disposables = new();
        private WorldGameplayRootViewModel _viewModel;

        public void Bind(WorldGameplayRootViewModel viewModel,
            GameplayUIManager gameplayUIManager,
            Subject<GameplayExitParams> exitSceneSignalSubj)
        {
            _viewModel = viewModel;

            viewModel.Player.Subscribe(heroViewModel => CreatePlayer(heroViewModel, gameplayUIManager));
            viewModel.CameraViewModel.Subscribe(cvm => CreateCamera(cvm, _playerView));
            foreach (var characterViewModel in viewModel.AllCharacters)
                CreateCharacter(characterViewModel, gameplayUIManager);
            foreach (var storageViewModel in viewModel.AllStorages)
                CreateStorage(storageViewModel, gameplayUIManager);
            foreach (var mapTransferViewModel in viewModel.AllMapTransfers)
                CreateMapTransfer(mapTransferViewModel, exitSceneSignalSubj);
            foreach (var enemySpawnViewModel in viewModel.AllSpawns) CreateSpawnTrigger(enemySpawnViewModel);

            _disposables.Add(viewModel.Player);
            _disposables.Add(viewModel.AllCharacters.ObserveAdd()
                .Subscribe(e => CreateCharacter(e.Value, gameplayUIManager)));
            _disposables.Add(viewModel.AllStorages.ObserveAdd().Subscribe(e =>
                CreateStorage(e.Value, gameplayUIManager)));
            _disposables.Add(viewModel.AllMapTransfers.ObserveAdd()
                .Subscribe(e => CreateMapTransfer(e.Value, exitSceneSignalSubj)));
            _disposables.Add(viewModel.AllSpawns.ObserveAdd()
                .Subscribe(e => CreateSpawnTrigger(e.Value)));
            _disposables.Add(viewModel.AllCharacters.ObserveRemove()
                .Subscribe(e => DestroyCharacter(e.Value)));
            _disposables.Add(viewModel.AllStorages.ObserveRemove()
                .Subscribe(e => DestroyStorage(e.Value)));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void CreateStorage(StorageViewModel storageViewModel, GameplayUIManager gameplayUIManager)
        {
            var entityType = storageViewModel.EntityType;
            var prefabCharacterLevelPath =
                $"Prefabs/Gameplay/World/Entities/Storages/{entityType}";
            var characterPrefab = Resources.Load<StorageView>(prefabCharacterLevelPath);

            var createdStorage = Instantiate(characterPrefab);
            createdStorage.Bind(storageViewModel, gameplayUIManager);

            _createStoragesMap[storageViewModel.Id] = createdStorage;
        }

        private void DestroyStorage(StorageViewModel storageViewModel)
        {
            if (_createStoragesMap.TryGetValue(storageViewModel.Id, out var storageView))
            {
                if (storageView != null)
                {
                    Destroy(storageView.gameObject);
                }
                _createCharactersMap.Remove(storageViewModel.Id);
            }
        }

        private void CreateCamera(CameraViewModel cameraViewModel, PlayerView hero)
        {
            var prefabCameraPath = "Prefabs/Gameplay/World/Entities/Camera/VirtualCamera";
            var cameraPrefab = Resources.Load<CameraView>(prefabCameraPath);

            var cameraBinder = Instantiate(cameraPrefab);
            cameraBinder.Bind(cameraViewModel, hero);
            _camera = cameraBinder;
        }

        private void CreatePlayer(PlayerViewModel playerViewModel, GameplayUIManager gameplayUIManager)
        {
            var prefabHeroPath = "Prefabs/Gameplay/World/Entities/Characters/Player";
            var heroPrefab = Resources.Load<PlayerView>(prefabHeroPath);

            var heroBinder = Instantiate(heroPrefab);
            heroBinder.Bind(playerViewModel, gameplayUIManager);
            _playerView = heroBinder;
        }

        private void CreateCharacter(CharacterViewModel characterViewModel, GameplayUIManager gameplayUIManager)
        {
            //создает CharacterView
            // для примера:
            var characterLevel = characterViewModel.Level.CurrentValue;
            //
            var characterType = characterViewModel.Type;
            var prefabCharacterLevelPath =
                $"Prefabs/Gameplay/World/Entities/Characters/{characterType}_{characterLevel}";
            var characterPrefab = Resources.Load<CharacterView>(prefabCharacterLevelPath);

            var createdCharacter = Instantiate(characterPrefab);
            createdCharacter.Bind(characterViewModel, gameplayUIManager);

            _createCharactersMap[characterViewModel.CharacterEntityId] = createdCharacter;
        }

        private void DestroyCharacter(CharacterViewModel characterViewModel)
        {
            if (_createCharactersMap.TryGetValue(characterViewModel.CharacterEntityId, out var characterBinder))
            {
                Destroy(characterBinder.gameObject);
                _createCharactersMap.Remove(characterViewModel.CharacterEntityId);
            }
        }

        private void CreateMapTransfer(MapTransferViewModel transferViewModel,
            Subject<GameplayExitParams> exitSceneSignal)
        {
            var transferId = transferViewModel.MapId;
            var prefabMapTransferPath = "Prefabs/Gameplay/World/Entities/MapTransfers/MapTransfer";
            var mapTransferPrefab = Resources.Load<MapTransferBinder>(prefabMapTransferPath);

            var createdMapTransfer = Instantiate(mapTransferPrefab);
            createdMapTransfer.Bind(exitSceneSignal, transferViewModel);

            _createMapTransfersMap[transferId] = createdMapTransfer;
        }

        private void CreateSpawnTrigger(EnemySpawnViewModel spawnViewModel)
        {
            var spawnId = spawnViewModel.Id;
            var prefabSpawnPath = "Prefabs/Gameplay/World/Entities/Spawns/SpawnTrigger";
            var spawnPrefab = Resources.Load<EnemySpawnView>(prefabSpawnPath);

            var createdSpawn = Instantiate(spawnPrefab);
            createdSpawn.Bind(spawnViewModel);

            _createSpawns[spawnId] = createdSpawn;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //_viewModel.HandleTestInput();
                Debug.Log(_playerView.ArsenalView.ActiveGun.WeaponType);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _viewModel.HandleTestInputTab();
            }
        }
    }
}