using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.View;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootBinder : MonoBehaviour
    {
        private readonly Dictionary<int, CharacterBinder> _createCharactersMap = new();
        private readonly Dictionary<MapId, MapTransferBinder> _createMapTransfersMap = new();
        private readonly Dictionary<string, EnemySpawnBinder> _createSpawns = new();
        private HeroBinder _hero;
        private CameraBinder _camera;
        private readonly CompositeDisposable _disposables = new();
        private WorldGameplayRootViewModel _viewModel;

        public void Bind(WorldGameplayRootViewModel viewModel, Subject<GameplayExitParams> exitSceneSignalSubj)
        {
            _viewModel = viewModel;

            viewModel.Hero.Subscribe(CreateHero);
            viewModel.CameraViewModel.Subscribe(cvm => CreateCamera(cvm, _hero));
            foreach (var characterViewModel in viewModel.AllCharacters) CreateCharacter(characterViewModel);
            foreach (var mapTransferViewModel in viewModel.AllMapTransfers)
                CreateMapTransfer(mapTransferViewModel, exitSceneSignalSubj);
            foreach (var enemySpawnViewModel in viewModel.AllSpawns) CreateSpawnTrigger(enemySpawnViewModel);

            _disposables.Add(viewModel.Hero);
            _disposables.Add(viewModel.AllCharacters.ObserveAdd()
                .Subscribe(e => CreateCharacter(e.Value)));

            _disposables.Add(viewModel.AllMapTransfers.ObserveAdd()
                .Subscribe(e => CreateMapTransfer(e.Value, exitSceneSignalSubj)));

            _disposables.Add(viewModel.AllSpawns.ObserveAdd()
                .Subscribe(e => CreateSpawnTrigger(e.Value)));

            _disposables.Add(viewModel.AllCharacters.ObserveRemove()
                .Subscribe(e => DestroyCharacter(e.Value)));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void CreateCamera(CameraViewModel cameraViewModel, HeroBinder hero)
        {
            var prefabCameraPath = "Prefabs/Gameplay/World/VirtualCamera";
            var cameraPrefab = Resources.Load<CameraBinder>(prefabCameraPath);

            var cameraBinder = Instantiate(cameraPrefab);
            cameraBinder.Bind(cameraViewModel, hero);
            _camera = cameraBinder;
        }

        private void CreateHero(HeroViewModel heroViewModel)
        {
            var prefabHeroPath = "Prefabs/Gameplay/World/Characters/Hero";
            var heroPrefab = Resources.Load<HeroBinder>(prefabHeroPath);

            var heroBinder = Instantiate(heroPrefab);
            heroBinder.Bind(heroViewModel);
            _hero = heroBinder;
        }

        private void CreateCharacter(CharacterViewModel characterViewModel)
        {
            //создает CharacterView
            // для примера:
            var characterLevel = characterViewModel.Level.CurrentValue;
            //
            var characterType = characterViewModel.TypeId;
            var prefabCharacterLevelPath =
                $"Prefabs/Gameplay/World/Characters/Character_{characterType}_{characterLevel}";
            var characterPrefab = Resources.Load<CharacterBinder>(prefabCharacterLevelPath);

            var createdCharacter = Instantiate(characterPrefab);
            createdCharacter.Bind(characterViewModel);

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
            var prefabMapTransferPath = "Prefabs/Gameplay/World/MapTransfers/MapTransfer";
            var mapTransferPrefab = Resources.Load<MapTransferBinder>(prefabMapTransferPath);

            var createdMapTransfer = Instantiate(mapTransferPrefab);
            createdMapTransfer.Bind(exitSceneSignal, transferViewModel);

            _createMapTransfersMap[transferId] = createdMapTransfer;
        }

        private void CreateSpawnTrigger(EnemySpawnViewModel spawnViewModel)
        {
            var spawnId = spawnViewModel.Id;
            var prefabSpawnPath = "Prefabs/Gameplay/World/Spawns/SpawnTrigger";
            var spawnPrefab = Resources.Load<EnemySpawnBinder>(prefabSpawnPath);

            var createdSpawn = Instantiate(spawnPrefab);
            createdSpawn.Bind(spawnViewModel);

            _createSpawns[spawnId] = createdSpawn;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _viewModel.HandleTestInput();
            }
        }
    }
}