using System;
using DI.Scripts;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.MainMenu.Root.View;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.MainMenu.Root
{
    public class MainMenuEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIMainMenuRootBinder _sceneUIRootPrefab;

        public Subject<MainMenuExitParams> Run(DIContainer mainMenuContainer, SceneEnterParams enterParams)
        {
            MainMenuRegistrations.Register(mainMenuContainer, enterParams);
            var mainMenuViewModelContainer = new DIContainer(mainMenuContainer);
            MainMenuViewModelsRegistrations.Register(mainMenuViewModelContainer);
            
            // Для теста:
            mainMenuViewModelContainer.Resolve<UIMainMenuRootViewModel>();
            
            var uiRoot = mainMenuContainer.Resolve<UIRootView>();
            var uiScene = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiScene.gameObject);

            var exitSignalSubj = new Subject<MainMenuExitParams>();
            uiScene.Bind(exitSignalSubj);

            //Debug.Log($"MAIN MENU ENTRY POINT: Run main menu scene. Result: {enterParams?.TargetSceneName}");
            
            return exitSignalSubj;
        }
        
    }
}