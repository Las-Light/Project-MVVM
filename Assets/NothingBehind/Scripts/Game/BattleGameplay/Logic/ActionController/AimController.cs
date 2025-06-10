using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using NothingBehind.Scripts.Game.BattleGameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController
{
    public class AimController : MonoBehaviour
    {
        public Transform AimPoint;
        [SerializeField] private float minDistToTargetSqrt = 9f;
        [SerializeField] private Transform pistolWeaponPose;
        [SerializeField] private Transform rifleWeaponPose;

        private AnimatorController _animatorController;
        private RigController _rigController;


        private void Start()
        {
            _animatorController = GetComponent<AnimatorController>();
            _rigController = GetComponent<RigController>();
        }

        public void Aim(ArsenalView arsenalView, bool isCheckWall)
        {
            pistolWeaponPose.rotation.SetLookRotation(Vector3.forward);
            rifleWeaponPose.rotation.SetLookRotation(Vector3.forward);

            //устанавливаем триггер для аниматора в зависимости от нажатия кнопки "прицеливание"
            _animatorController.Aim(true);

            if (!isCheckWall)
            {
                _rigController.SetRigAim(arsenalView.CurrentWeapon.WeaponType, true);
            }

            if (arsenalView.CurrentWeapon.WeaponType == WeaponType.Pistol && !arsenalView.IsReloading)
            {
                _rigController.SetRigLayerLeftHandIK(1, WeaponType.Pistol);
            }
        }

        public void RemoveAim(ArsenalView arsenalView)
        {
            //после прицеливания оружия может выкручиваться в руках, поэтому сбрасываем ротацию
            pistolWeaponPose.rotation = Quaternion.LookRotation(Vector3.forward);
            rifleWeaponPose.rotation = Quaternion.LookRotation(Vector3.forward);

            //задаём анимацию прицеливания
            _animatorController.Aim(false);

            _rigController.SetRigAim(arsenalView.CurrentWeapon.WeaponType, false);
            if (arsenalView.CurrentWeapon.WeaponType == WeaponType.Pistol)
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
                SetAimPointForward(aimPosition, gun);
            }
        }

        //устанавливает направление AimPointa на дефолтное по отношению к телу юнита
        public void SetAimPointForward()
        {
            AimPoint.position = Vector3.Lerp(AimPoint.position,
                new Vector3(transform.position.x, 1.2f, transform.position.z) + transform.forward * 8,
                Time.deltaTime * 20);
        }

        public void SetAimPointForward(Vector3 aimPosition, WeaponView gun)
        {
            AimPoint.position = Vector3.Lerp(AimPoint.position,
                aimPosition + transform.forward * gun.CheckDistanceToWall, Time.deltaTime * 20);
        }
    }
}