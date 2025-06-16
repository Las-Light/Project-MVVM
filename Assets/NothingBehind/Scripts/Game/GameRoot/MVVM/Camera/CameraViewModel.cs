using NothingBehind.Scripts.Game.GameRoot.Services;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay;

namespace NothingBehind.Scripts.Game.GameRoot.MVVM.Camera
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