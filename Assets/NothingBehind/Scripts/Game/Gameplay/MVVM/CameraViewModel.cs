using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Logic.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraViewModel
    {
        public readonly GameplayInputManager InputManager;
        public readonly GameplayCameraSettings CameraSettings;
        private readonly CameraService _cameraService;

        public CameraViewModel(CameraService cameraService, GameplayInputManager inputManager, GameplayCameraSettings cameraSettings)
        {
            _cameraService = cameraService;
            InputManager = inputManager;
            CameraSettings = cameraSettings;
        }
    }
}