using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI;
using NothingBehind.Scripts.Game.GameRoot;
using NothingBehind.Scripts.Game.GlobalMap.MVVM.UI;
using NothingBehind.Scripts.Game.GlobalMap.Root.View;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.Root
{
    public class GlobalMapEntryPoint : MonoBehaviour
    {
        [SerializeField] private UIGlobalMapRootBinder sceneUIRootPrefab;
        [SerializeField] private WorldGlobalMapRootView worldRootView;
        public Subject<GlobalMapExitParams> Run(DIContainer globalMapContainer, SceneEnterParams enterParams)
        {
            GlobalMapRegistrations.Register(globalMapContainer, enterParams);
            var globalMapViewModelsContainer = new DIContainer(globalMapContainer);
            GlobalMapViewModelsRegistrations.Register(globalMapViewModelsContainer);

            var exitSceneRequest =
                globalMapContainer.Resolve<Subject<GlobalMapExitParams>>(AppConstants.EXIT_SCENE_REQUEST_TAG);

            // Для теста:
            InitWorld(globalMapViewModelsContainer, exitSceneRequest);
            InitUI(globalMapViewModelsContainer);

            return exitSceneRequest;
        }
        
        private void InitWorld(DIContainer globalMapViewModelsContainer, Subject<GlobalMapExitParams> exitSceneRequest)
        {
            //Добавить сюда инициализацию мира (статик дату, героя, спавнеры врагов)
            worldRootView.Bind(
                globalMapViewModelsContainer.Resolve<WorldGlobalMapRootViewModel>(),
                globalMapViewModelsContainer.Resolve<GlobalMapUIManager>(),
                exitSceneRequest);
        }

        private void InitUI(DIContainer viewsContainer)
        {
            // Создали UI для сцены
            var uiRoot = viewsContainer.Resolve<UIRootView>();
            var uiSceneRootBinder = Instantiate(sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);
            
            // Запрашиваем рутовую вью модель и пихаем её в байндер, который создали
            var uiSceneRootViewModel = viewsContainer.Resolve<UIGlobalMapRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);
            
            // Можно открывать окошки
            var uiManager = viewsContainer.Resolve<GlobalMapUIManager>();
            uiManager.OpenScreenGameplay();
        }
    }
}