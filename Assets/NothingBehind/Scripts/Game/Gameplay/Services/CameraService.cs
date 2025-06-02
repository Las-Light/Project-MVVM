using NothingBehind.Scripts.Game.Gameplay.MVVM;
using NothingBehind.Scripts.Game.GameRoot.Services.InputManager;
using NothingBehind.Scripts.Game.Settings.Gameplay;
using R3;

namespace NothingBehind.Scripts.Game.Gameplay.Services
{
    public class CameraService
    {
        //Если будет более одной камеры (например сплитскрин на двоих игроков) тогда надо создать Dictionary с вьюхами и вьюмоделями 
        public ReadOnlyReactiveProperty<CameraViewModel> CameraViewModel => _cameraViewModel;

        private readonly ReactiveProperty<CameraViewModel> _cameraViewModel = new();
        private readonly ReadOnlyReactiveProperty<bool> _isRotateCameraLeft;
        private readonly ReadOnlyReactiveProperty<bool> _isRotateCameraRight;

        private bool _progressRotate;

        public CameraService(InputManager inputManager,
            GameplayCameraSettings cameraSettings)
        {
            CreateCameraViewModel(inputManager, cameraSettings);

        }

        private void CreateCameraViewModel(InputManager inputManager, GameplayCameraSettings cameraSettings)
        {
            var cameraViewModel = new CameraViewModel(this, inputManager, cameraSettings);

            _cameraViewModel.OnNext(cameraViewModel);
        }
    }
}