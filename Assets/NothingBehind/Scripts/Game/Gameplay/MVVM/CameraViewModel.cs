using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraViewModel
    {
        public readonly InputManager InputManager;
        public readonly GameplayCameraSettings CameraSettings;
        private readonly CameraService _cameraService;

        public CameraViewModel(CameraService cameraService, InputManager inputManager, GameplayCameraSettings cameraSettings)
        {
            _cameraService = cameraService;
            InputManager = inputManager;
            CameraSettings = cameraSettings;
        }
    }
}