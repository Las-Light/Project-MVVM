using System.Linq;
using DI.Scripts;
using NothingBehind.Scripts.Game.Common;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.Inventories;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.PopupA;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.PopupB;
using NothingBehind.Scripts.Game.Gameplay.MVVM.UI.ScreenGameplay;
using NothingBehind.Scripts.Game.Gameplay.Root;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.MVVM.UI;
using NothingBehind.Scripts.Utils;
using R3;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM.UI
{
    public class GameplayUIManager : UIManager
    {
        private readonly Subject<GameplayExitParams> _exitSceneRequest;
        private readonly Subject<ExitInventoryRequestResult> _exitInventoryRequest; // Реквест на закрытие инвентаря передается в InventoryUIViewModel
        private readonly InventoryService _inventoryService;
        private readonly EquipmentService _equipmentService;
        private readonly StorageService _storageService;
        private readonly PlayerService _playerService;
        private readonly GameplayInputManager _gameplayInputManager;

        public GameplayUIManager(DIContainer container) : base(container)
        {
            _exitSceneRequest = container.Resolve<Subject<GameplayExitParams>>(AppConstants.EXIT_SCENE_REQUEST_TAG);
            _exitInventoryRequest = container.Resolve<Subject<ExitInventoryRequestResult>>(AppConstants.EXIT_INVENTORY_REQUEST_TAG);
            _inventoryService = container.Resolve<InventoryService>();
            _equipmentService = container.Resolve<EquipmentService>();
            _storageService = container.Resolve<StorageService>();
            _playerService = container.Resolve<PlayerService>();
            _gameplayInputManager = container.Resolve<GameplayInputManager>();
        }

        public ScreenGameplayViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGameplayViewModel(this, _exitSceneRequest, _playerService);
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();

            rootUI.OpenSreen(viewModel);

            return viewModel;
        }

        public InventoryUIViewModel OpenInventory(int targetOwnerId, int ownerId, Vector3 position)
        {
            var inventoryUI = new InventoryUIViewModel(_inventoryService,
                _equipmentService, 
                _storageService, 
                ownerId,
                position,
                targetOwnerId,
                _exitInventoryRequest,
                _gameplayInputManager);
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();
            
            _gameplayInputManager.UIInputEnabled();
            rootUI.OpenPopup(inventoryUI);
            return inventoryUI;
        }

        public void CloseInventory()
        {
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();
            var inventoryUIViewModel = rootUI.OpenedPopups.FirstOrDefault(model => model is InventoryUIViewModel);
            if (inventoryUIViewModel != null)
            {
                rootUI.ClosePopup(inventoryUIViewModel);
            }
        }

        public PopupAViewModel OpenPopupA()
        {
            var a = new PopupAViewModel();
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();

            rootUI.OpenPopup(a);

            return a;
        }

        public PopupBViewModel OpenPopupB()
        {
            var b = new PopupBViewModel();
            var rootUI = Container.Resolve<UIGameplayRootViewModel>();

            rootUI.OpenPopup(b);

            return b;
        }
    }
}