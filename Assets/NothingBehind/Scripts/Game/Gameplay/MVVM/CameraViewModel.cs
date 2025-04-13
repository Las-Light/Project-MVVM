using NothingBehind.Scripts.Game.Gameplay.Logic;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using Unity.Cinemachine;

namespace NothingBehind.Scripts.Game.Gameplay.MVVM
{
    public class CameraViewModel
    {
        private readonly CameraManager _cameraManager;
        
        private CinemachineCamera _cinemachineCamera;
        private CameraView _cameraView;

        public CameraViewModel(CameraManager cameraManager)
        {
            _cameraManager = cameraManager;
        }

        public void SetCameraViewWithComponent(CameraView cameraView, PlayerView heroView)
        {
            _cameraView = cameraView;
            _cinemachineCamera = cameraView.GetComponent<CinemachineCamera>();
            var cinemachineFollow = cameraView.GetComponent<CinemachineFollow>();
            _cameraManager.BindCameraViewComponent(cameraView, _cinemachineCamera, cinemachineFollow);
            
            _cameraManager.CameraFollow(heroView.transform);
        }
    }
}