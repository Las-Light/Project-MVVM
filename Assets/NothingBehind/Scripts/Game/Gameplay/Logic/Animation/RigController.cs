using System;
using System.Collections;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Animation
{
    public class RigController : MonoBehaviour
    {
        [Space(10)] [Header("Rigging")] [SerializeField]
        private Rig riflePoseRig;

        [SerializeField] private Rig pistolPoseRig;
        [SerializeField] private Rig rifleAimRig;
        [SerializeField] private Rig pistolAimRig;
        [SerializeField] private Rig rifleHandRig;
        [SerializeField] private TwoBoneIKConstraint rifleRightHandRig;
        [SerializeField] private TwoBoneIKConstraint rifleLeftHandRig;
        [SerializeField] private Rig pistolHandRig;
        [SerializeField] private TwoBoneIKConstraint pistolRightHandRig;
        [SerializeField] private TwoBoneIKConstraint pistolLeftHandRig;
        [SerializeField] private Rig clipWallRig;


        //устанавливает веса рига если основное и второстепенное оружие убрано
        public void SetRigWeightUnarmed()
        {
            clipWallRig.weight = 0;
            riflePoseRig.weight = 0;
            pistolPoseRig.weight = 0;
            rifleAimRig.weight = 0;
            pistolAimRig.weight = 0;
            pistolHandRig.weight = 0;
            rifleHandRig.weight = 0;
        }

        //устанавливает веса рига если выбрано оружие пистолет
        public void SetRigWeightGetPistol()
        {
            clipWallRig.weight = 0;
            riflePoseRig.weight = 0;
            rifleAimRig.weight = 0;
            rifleHandRig.weight = 0;
            pistolAimRig.weight = 0;
            pistolPoseRig.weight = 1;
            pistolHandRig.weight = 1;
        }

        //устанавливает веса рига если выбрано оружие ружье
        public void SetRigWeightGetRifle()
        {
            clipWallRig.weight = 0;
            pistolPoseRig.weight = 0;
            pistolAimRig.weight = 0;
            pistolHandRig.weight = 0;
            rifleAimRig.weight = 0;
            riflePoseRig.weight = 1;
            rifleHandRig.weight = 1;
        }


        //задает вес рига rifleAimRig, если нужно прицелиться
        public void AimRifleRig(bool takeAim)
        {
            if (takeAim)
            {
                StartCoroutine(SetWeightRig(1, rifleAimRig));
            }
            //rifleAimRig.weight = Mathf.Lerp(rifleAimRig.weight, 1, Time.deltaTime * 15f);
            else
                StartCoroutine(SetWeightRig(0, rifleAimRig));
            //rifleAimRig.weight = Mathf.Lerp(rifleAimRig.weight, 0, Time.deltaTime * 15f);
        }

        //задает вес рига pistolAimRig, если нужно прицелиться
        public void AimPistolRig(bool takeAim)
        {
            if (takeAim)
            {
                StartCoroutine(SetWeightRig(1, pistolAimRig));
            }
            else
            {
                StartCoroutine(SetWeightRig(0, pistolAimRig));
            }
        }


        //метод задает вес ригу ClipWallRig, если перед игроком стена, то поднимает оружие вверх
        public void ClipWallRig(bool checkWall)
        {
            if (checkWall)
            {
                clipWallRig.weight = Mathf.Lerp(clipWallRig.weight, 1, Time.deltaTime * 15f);
            }
            else
            {
                clipWallRig.weight = Mathf.Lerp(clipWallRig.weight, 0, Time.deltaTime * 15f);
            }
        }

        //метод который отвязвает руки в инверскинематик от ружья 
        public void SetRigLayerHandIK(float value)
        {
            rifleHandRig.weight = value;
            //rifleHandRig.weight = Mathf.Lerp(rifleHandRig.weight, value, Time.deltaTime * 20f);
        }

        public void SetRigLayerRightHandIK(float value)
        {
            rifleHandRig.weight = Mathf.Lerp(rifleHandRig.weight, value, Time.deltaTime * 200f);
        }
        
        //TODO: раскоментировать после реализации Weapon System

        public void SetRigLayerLeftHandIK(float destinationValue, WeaponType weaponType)
        {
            if (weaponType == WeaponType.Rifle)
            {
                rifleLeftHandRig.data.targetPositionWeight = Mathf.Lerp(rifleLeftHandRig.data.targetPositionWeight,
                     destinationValue, Time.deltaTime * 200f);
            }
        
            if (weaponType == WeaponType.Pistol)
            {
                pistolLeftHandRig.data.targetPositionWeight = Mathf.Lerp(pistolLeftHandRig.data.targetPositionWeight,
                    destinationValue, Time.deltaTime * 200f);
                pistolLeftHandRig.data.hintWeight =
                    Mathf.Lerp(pistolLeftHandRig.data.hintWeight, destinationValue, Time.deltaTime * 200f);
                pistolLeftHandRig.data.targetRotationWeight = Mathf.Lerp(pistolLeftHandRig.data.targetRotationWeight,
                    destinationValue, Time.deltaTime * 200f);
            }
        }

        //устанавливает вес рига если игрок прицелился
        public void SetRigAim(WeaponType weaponType)
        {
            if (weaponType == WeaponType.Pistol)
            {
                AimPistolRig(true);
            }
            else if (weaponType == WeaponType.Rifle)
            {
                AimRifleRig(true);
            }
        }
        
        private IEnumerator SetWeightRig(float destinationWeight, Rig rig)
        {
            while (Math.Abs(rig.weight - destinationWeight) > 0.01f)
            {
                rig.weight = Mathf.Lerp(rig.weight, destinationWeight, Time.deltaTime * 15f);
            }

            yield return null;
        }
        
        private IEnumerator SetWeightRig(float destinationWeight, TwoBoneIKConstraint rig)
        {
            yield return null;
            while (Math.Abs(rig.data.targetPositionWeight - destinationWeight) > 0.01f)
            {
                rig.data.targetPositionWeight = Mathf.Lerp(rig.data.targetPositionWeight, destinationWeight, Time.deltaTime * 200f);
                rig.data.hintWeight = Mathf.Lerp(rig.data.hintWeight, destinationWeight, Time.deltaTime * 200f);
                rig.data.targetRotationWeight = Mathf.Lerp(rig.data.targetRotationWeight, destinationWeight, Time.deltaTime * 200f);
            }

            yield return null;
        }
    }
}