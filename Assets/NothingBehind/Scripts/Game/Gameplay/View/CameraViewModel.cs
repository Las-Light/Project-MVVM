using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.Services;
using NothingBehind.Scripts.Game.Gameplay.View.Characters;
using Unity.Cinemachine;

namespace NothingBehind.Scripts.Game.Gameplay.View
{
    public class CameraViewModel
    {
        private readonly CameraManager _cameraManager;
        
        private CinemachineCamera _cinemachineCamera;
        private CameraBinder _cameraView;

        public CameraViewModel(CameraManager cameraManager)
        {
            _cameraManager = cameraManager;
        }

        public void SetCameraViewWithComponent(CameraBinder cameraView, HeroBinder heroView)
        {
            _cameraView = cameraView;
            _cinemachineCamera = cameraView.GetComponent<CinemachineCamera>();
            var cinemachineFollow = cameraView.GetComponent<CinemachineFollow>();
            _cameraManager.BindCameraViewComponent(cameraView, _cinemachineCamera, cinemachineFollow);
            
            _cameraManager.CameraFollow(heroView.transform);
        }
    }
}