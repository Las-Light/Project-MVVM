using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using Unity.Cinemachine;

namespace NothingBehind.Scripts.Game.Gameplay.View
{
    public class CameraViewModel
    {
        private readonly CameraService _cameraService;
        
        private CinemachineCamera _cinemachineCamera;
        private CameraBinder _cameraView;

        public CameraViewModel(CameraService cameraService)
        {
            _cameraService = cameraService;
        }

        public void SetCameraViewWithComponent(CameraBinder cameraView, HeroBinder heroView)
        {
            _cameraView = cameraView;
            _cinemachineCamera = cameraView.GetComponent<CinemachineCamera>();
            var cinemachineFollow = cameraView.GetComponent<CinemachineFollow>();
            _cameraService.BindCameraViewComponent(cameraView, _cinemachineCamera, cinemachineFollow);
            
            _cameraService.CameraFollow(heroView.transform);
        }
    }
}