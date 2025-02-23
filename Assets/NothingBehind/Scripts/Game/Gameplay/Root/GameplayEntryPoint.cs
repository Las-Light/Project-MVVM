using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Root.View;
using NothingBehind.Scripts.Game.Gameplay.View.UI;
using NothingBehind.Scripts.Game.GameRoot;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Root
{
    public class GameplayEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGameplayRootBinder _sceneUIRootPrefab;
        [SerializeField] private WorldGameplayRootBinder _worldRootBinder;

        public Subject<GameplayExitParams> Run(DIContainer gameplayContainer, SceneEnterParams enterParams)
        {
            GameplayRegistrations.Register(gameplayContainer, enterParams);
            var gameplayViewModelsContainer = new DIContainer(gameplayContainer);
            GameplayViewModelsRegistrations.Register(gameplayViewModelsContainer);

            var exitSceneRequest =
                gameplayContainer.Resolve<Subject<GameplayExitParams>>(AppConstants.EXIT_SCENE_REQUEST_TAG);

            // Для теста:
            InitWorld(gameplayViewModelsContainer, exitSceneRequest);
            InitUI(gameplayViewModelsContainer);

            return exitSceneRequest;
        }

        private void InitWorld(DIContainer gameplayViewModelsContainer, Subject<GameplayExitParams> exitSceneRequest)
        {
            //Добавить сюда инициализацию мира (статик дату, героя, спавнеры врагов)
            _worldRootBinder.Bind(
                gameplayViewModelsContainer.Resolve<WorldGameplayRootViewModel>(),
                exitSceneRequest);
        }

        private void InitUI(DIContainer viewsContainer)
        {
            // Создали UI для сцены
            var uiRoot = viewsContainer.Resolve<UIRootView>();
            var uiSceneRootBinder = Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);
            
            // Запрашиваем рутовую вью модель и пихаем её в байндер, который создали
            var uiSceneRootViewModel = viewsContainer.Resolve<UIGameplayRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);
            
            // Можно открывать окошки
            var uiManager = viewsContainer.Resolve<GameplayUIManager>();
            uiManager.OpenScreenGameplay();
        }
    }
}