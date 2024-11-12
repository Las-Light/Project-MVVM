using System;
using DI.Scripts;
using NothingBehind.Scripts.Game.Gameplay.Commands;
using NothingBehind.Scripts.Game.Gameplay.Root.View;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.MainMenu.Root;
using NothingBehind.Scripts.Game.State;
using NothingBehind.Scripts.Game.State.Commands;
using NothingBehind.Scripts.Game.State.Root;
using ObservableCollections;
using R3;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;
        [SerializeField] private WorldGameplayRootBinder _worldRootBinder;

        public Observable<GameplayExitParams> Run(DIContainer gameplayContainer, GameplayEnterParams enterParams)
        {
            GameplayRegistrations.Register(gameplayContainer, enterParams);
            var gameplayViewModelsContainer = new DIContainer(gameplayContainer);
            GameplayViewModelsRegistrations.Register(gameplayViewModelsContainer);

            // Для теста:

            var gameStateProvider = gameplayContainer.Resolve<IGameStateProvider>();

            //

            gameStateProvider.GameState.Characters.ObserveAdd().Subscribe(e =>
            {
                var character = e.Value;
                Debug.Log("Character created. TypeId: " + character.TypeId
                                                        + " Id: " + character.Id
                                                        + ", Position: " + character.Position);
            });

            // Для теста:
            // Вытаскиваем из контейнера CharactersService

            var charactersService = gameplayContainer.Resolve<CharactersService>();

            charactersService.CreateCharacter("Dummy", GetRandomPosition());
            charactersService.CreateCharacter("Dummy", GetRandomPosition());
            charactersService.CreateCharacter("Dummy", GetRandomPosition());

            // Для теста:
            _worldRootBinder.Bind(gameplayViewModelsContainer.Resolve<WorldGameplayRootViewModel>());

            gameplayViewModelsContainer.Resolve<UIGameplayRootViewModel>();

            var uiRoot = gameplayContainer.Resolve<UIRootView>();
            var uiScene = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiScene.gameObject);

            var exitSceneSignalSubj = new Subject<Unit>();

            uiScene.Bind(exitSceneSignalSubj);

            Debug.Log($"GAMEPLAY ENTRY POINT: save file name = {enterParams.SaveFileName}");

            var mainMenuEnterParams = new MainMenuEnterParams("EnterParamsCheck");
            var gameplayExitParams = new GameplayExitParams(mainMenuEnterParams);
            var exitToMainMenuSceneSignal = exitSceneSignalSubj.Select(_ => gameplayExitParams);

            return exitToMainMenuSceneSignal;
        }

        private Vector3Int GetRandomPosition()
        {
            var rX = Random.Range(-10, 10);
            var rY = Random.Range(-10, 10);
            var rPosition = new Vector3Int(rX, rY, 0);

            return rPosition;
        }
    }
}