using System;
using System.Collections;
using System.Collections.Generic;
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

        private Dictionary<Rig, Coroutine> _activeWeightRoutines = new Dictionary<Rig, Coroutine>();

        private const float LERP_SPEED_FAST = 200f;
        private const float LERP_SPEED_NORMAL = 15f;
        private const float WEIGHT_THRESHOLD = 0.01f;

        // Общие настройки для всех ригов
        public void SetRigWeightUnarmed()
        {
            SetAllRigWeights(0f);
        }

        public void SetRigWeightGetPistol()
        {
            SetAllRigWeights(0f);
            pistolPoseRig.weight = 1f;
            pistolHandRig.weight = 1f;
        }

        public void SetRigWeightGetRifle()
        {
            SetAllRigWeights(0f);
            riflePoseRig.weight = 1f;
            rifleHandRig.weight = 1f;
        }

        private void SetAllRigWeights(float weight)
        {
            clipWallRig.weight = weight;
            riflePoseRig.weight = weight;
            pistolPoseRig.weight = weight;
            rifleAimRig.weight = weight;
            pistolAimRig.weight = weight;
            pistolHandRig.weight = weight;
            rifleHandRig.weight = weight;
        }

        // Методы для прицеливания
        public void AimRifleRig(bool takeAim) => StartCoroutine(SetWeightRig(takeAim ? 1 : 0, rifleAimRig));
        public void AimPistolRig(bool takeAim) => StartCoroutine(SetWeightRig(takeAim ? 1 : 0, pistolAimRig));

        public void SetRigAim(WeaponType weaponType)
        {
            if (weaponType == WeaponType.Pistol) AimPistolRig(true);
            else if (weaponType == WeaponType.Rifle) AimRifleRig(true);
        }

        // Метод для обработки стены
        public void ClipWallRig(bool checkWall)
        {
            float targetWeight = checkWall ? 1f : 0f;
            clipWallRig.weight = Mathf.Lerp(clipWallRig.weight, targetWeight, Time.deltaTime * LERP_SPEED_NORMAL);
        }

        // Методы для управления руками
        public void SetRigLayerHandIK(float value) => rifleHandRig.weight = value;

        public void SetRigLayerRightHandIK(float value) => rifleHandRig.weight =
            Mathf.Lerp(rifleHandRig.weight, value, Time.deltaTime * LERP_SPEED_FAST);

        public void SetRigLayerLeftHandIK(float destinationValue, WeaponType weaponType)
        {
            float lerpValue = Time.deltaTime * LERP_SPEED_FAST;

            if (weaponType == WeaponType.Rifle)
            {
                var data = rifleLeftHandRig.data;
                data.targetPositionWeight = Mathf.Lerp(data.targetPositionWeight, destinationValue, lerpValue);
                rifleLeftHandRig.data = data; // Важно: присваиваем обратно!
            }
            else if (weaponType == WeaponType.Pistol)
            {
                var data = pistolLeftHandRig.data;
                data.targetPositionWeight = Mathf.Lerp(data.targetPositionWeight, destinationValue, lerpValue);
                data.hintWeight = Mathf.Lerp(data.hintWeight, destinationValue, lerpValue);
                data.targetRotationWeight = Mathf.Lerp(data.targetRotationWeight, destinationValue, lerpValue);
                pistolLeftHandRig.data = data; // Применяем изменения
            }
        }

        private IEnumerator SetWeightRig(float destinationWeight, Rig rig)
        {
            // Если уже есть активная корутина для этого рига - останавливаем её
            if (_activeWeightRoutines.TryGetValue(rig, out Coroutine runningRoutine))
            {
                if (runningRoutine != null)
                    StopCoroutine(runningRoutine);
            }

            // Запускаем новую корутину и сохраняем ссылку
            Coroutine newRoutine = StartCoroutine(SetWeightRigRoutine(destinationWeight, rig));
            _activeWeightRoutines[rig] = newRoutine;

            yield return newRoutine;
        }

        private IEnumerator SetWeightRigRoutine(float destinationWeight, Rig rig)
        {
            while (Mathf.Abs(rig.weight - destinationWeight) > WEIGHT_THRESHOLD)
            {
                rig.weight = Mathf.Lerp(rig.weight, destinationWeight, Time.deltaTime * LERP_SPEED_NORMAL);
                yield return null;
            }

            rig.weight = destinationWeight; // Финализируем значение
            _activeWeightRoutines.Remove(rig); // Удаляем завершенную корутину из словаря
        }
    }
}