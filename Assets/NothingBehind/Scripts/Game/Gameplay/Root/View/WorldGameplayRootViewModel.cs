using System.Linq;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using NothingBehind.Scripts.Game.Gameplay.View.Maps;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.GameResources;
using NothingBehind.Scripts.Game.State.Maps;
using ObservableCollections;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root.View
{
    public class WorldGameplayRootViewModel
    {
        private readonly CharactersService _charactersService;
        public readonly IObservableCollection<CharacterViewModel> AllCharacters;
        public readonly IObservableCollection<MapTransferViewModel> AllMapTransfers;
        public readonly IObservableCollection<EnemySpawnViewModel> AllSpawns;

        public WorldGameplayRootViewModel(
            CharactersService charactersService,
            IGameStateProvider gameStateProvider,
            ResourcesService resourcesService,
            SpawnService spawnService,
            InitialMapStateService initialMapService)
        {
            _charactersService = charactersService;
            AllCharacters = charactersService.AllCharacters;
            AllMapTransfers = initialMapService.MapTransfers;
            AllSpawns = spawnService.EnemySpawns;

            resourcesService.ObserveResource(ResourceType.SoftCurrency)
                .Subscribe(newValue => Debug.Log($"SoftCurrency: {newValue}"));
            resourcesService.ObserveResource(ResourceType.HardCurrency)
                .Subscribe(newValue => Debug.Log($"HardCurrency: {newValue}"));
        }

        public void HandleTestInput()
        {
            _charactersService.CreateCharacter(
                "Dummy",
                Random.Range(1, 3),
                new Vector3Int(Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 6)));
            foreach (var characterViewModel in AllCharacters)
            {
                Debug.Log(characterViewModel.TypeId + " + " + characterViewModel.Level + " + " +
                          characterViewModel.Health);
            }
        }
    }
}