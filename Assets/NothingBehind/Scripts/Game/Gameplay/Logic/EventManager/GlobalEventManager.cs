using System;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.EventManager
{
    public static class GlobalEventManager
    {
        //TODO: REfactoring this class
        public static event Action<GameObject> OnEnemyKilled;
        public static event Action<GameObject, GameObject> OnSeenTarget;
        public static event Action<GameObject, GameObject> OnLostTarget;
        public static event Action<GameObject> OnHearingSound;
        public static event Action<GameObject> OnFoundAboutTarget;
        public static event Action<GameObject> OnAttackTarget;
        public static event Action<GameObject> OnUpdatePosition;
        
        
        public static event Action<GameObject> OnDogCommandAttack;
        public static event Action OnDogCommandComeback;
        public static event Action OnDogCommandBark;
        public static event Action OnDogCommandWait;

        //HUMAN EVENTS
        public static void SendEnemyKilled(GameObject enemy)
        {
            OnEnemyKilled?.Invoke(enemy);
        }
        
        public static void SendSeenTarget(GameObject unit, GameObject target)
        {
            OnSeenTarget?.Invoke(unit, target);
        }

        public static void SendLostTarget(GameObject unit, GameObject target)
        {
            OnLostTarget?.Invoke(unit, target);
        }
        public static void SendHearingSound(GameObject unit)
        {
            OnHearingSound?.Invoke(unit);
        }

        public static void SendFoundAboutTarget(GameObject unit)
        {
            OnFoundAboutTarget?.Invoke(unit);
        }

        public static void SendAttackTarget(GameObject unit)
        {
            OnAttackTarget?.Invoke(unit);
        }
        public static void SendUpdatePosition(GameObject unit)
        {
            OnUpdatePosition?.Invoke(unit);
        }
    }
}