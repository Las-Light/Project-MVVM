using NothingBehind.Scripts.Game.Gameplay.Logic.Animation;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Player;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Player
{
    public class AimController : MonoBehaviour
    {
        public bool AimAssistON;
        public Transform AimPoint;
        [SerializeField] private float minDistToTargetSqrt = 9f;

        private PlayerView _playerView;
        private ArsenalView _arsenalView;
        private AnimatorController _animatorController;
        private RigController _rigController;


        private void Start()
        {
            _playerView = GetComponent<PlayerView>();
            _arsenalView = _playerView.ArsenalView;
            _animatorController = GetComponent<AnimatorController>();
            _rigController = GetComponent<RigController>();
        }

        public void Aim()
        {
            //устанавливаем поле isAim если нажата кнопка "Прицелиться"
            _playerView.IsAim = true;

            //устанавливаем триггер для аниматора в зависимости от нажатия кнопки "прицеливание"
            _animatorController.Aim(_playerView.IsAim);

            if (!_playerView.IsCheckWall)
            {
                _rigController.SetRigAim(_arsenalView.ActiveGun.WeaponType);
            }

            if (_arsenalView.ActiveGun.WeaponType == WeaponType.Pistol && !_arsenalView.IsReloading)
            {
                _rigController.SetRigLayerLeftHandIK(1, WeaponType.Pistol);
            }
        }

        public void RemoveAim()
        {
            //устанавливаем поле isAim false, если отпущена кнопка "Прицелиться"
            _playerView.IsAim = false;

            //задаём анимацию прицеливания
            _animatorController.Aim(_playerView.IsAim);

            _rigController.AimRifleRig(false);
            _rigController.AimPistolRig(false);
            if (_arsenalView.ActiveGun.WeaponType == WeaponType.Pistol)
            {
                _rigController.SetRigLayerLeftHandIK(0, WeaponType.Pistol);
            }
        }


        //устанавливает позицию AimPointa на цель
        public void SetAimPointPosition(Vector3 aimPosition, WeaponView gun)
        {
            
            float disToTarget = (transform.position - new Vector3(aimPosition.x, transform.position.y, aimPosition.z))
                .sqrMagnitude;
            if (disToTarget >= gun.CheckDistanceToWall)
            {
                AimPoint.position = aimPosition;
                //AimPoint.position = Vector3.Lerp(AimPoint.position, aimPosition, Time.deltaTime * 1000);
            }
            else
            {
                SetAimPointForward(aimPosition);
            }
        }

        //устанавливает направление AimPointa на дефолтное по отношению к телу юнита
        private void SetAimPointForward()
        {
            AimPoint.position = Vector3.Lerp(AimPoint.position,
                new Vector3(transform.position.x, 1.2f, transform.position.z) + transform.forward * 8,
                Time.deltaTime * 20);
        }

        private void SetAimPointForward(Vector3 aimPosition)
        {
            AimPoint.position = Vector3.Lerp(AimPoint.position,
                aimPosition + transform.forward * _arsenalView.ActiveGun.CheckDistanceToWall, Time.deltaTime * 20);
        }

        //направляет AimPoint к близшайшей цели, если целей нет в зоне видимости то AimPoint возвращается дефолтное состояние
        public void AimPointTargetGamepad()
        {
            if (AimAssistON)
            {
                if (_playerView.CurrentEnemy)
                    SetAimPointPosition(_playerView.CurrentEnemy.transform.GetChild(0).position,
                        _arsenalView.ActiveGun);
                else
                    SetAimPointForward();
            }
            else
                SetAimPointForward();
        }

        public void AimPointTargetMouse(Vector3 mouseWorldPosition)
        {
            if (AimAssistON)
            {
                if (_playerView.CurrentEnemy)
                    SetAimPointPosition(_playerView.CurrentEnemy.transform.GetChild(0).position, _arsenalView.ActiveGun);
                else
                    SetAimPointPosition(mouseWorldPosition, _arsenalView.ActiveGun);
            }
            else
                SetAimPointPosition(mouseWorldPosition, _arsenalView.ActiveGun);
        }
    }
}