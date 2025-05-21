using System;
using System.Collections.Generic;
using NothingBehind.Scripts.Game.Gameplay.Logic.PatrolSystem;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Characters;
using NothingBehind.Scripts.Game.Gameplay.MVVM.Weapons;
using NothingBehind.Scripts.Game.State.Weapons.TypeData;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Data
{
    [Serializable]
    public class EnemyData: MonoBehaviour
    {
        private ArsenalView _arsenalView;

        //TODO сделать не монобехом, а через DI
        public GameObject CurrentEnemy;
        public TargetType EnemyType;
        public Points CurrentCover;
        public Vector3 CurrentMovementPos;
        public Vector3 TargetLastKnownPosition;
        public Vector3 TargetLastKnownDirection;
        public float TargetLastKnownDetectionTime;
        public Vector3 HeardSoundPosition;
        public float SoundDetectionTime;
        public WeaponType WeaponType => _arsenalView.ActiveGun.WeaponType;
        public SoundType SoundType;

        public List<GameObject> VisibleTargets = new List<GameObject>();
        public List<Points> FoundInvestigatePoint  = new List<Points>();
        public List<Vector3> PassedInvestigatePos  = new List<Vector3>();
        public List<Points> FoundCoverPos = new List<Points>();
        public List<Points> PassedCoverPos  = new List<Points>();
        
        public Dictionary<GameObject, Vector3> OccupiedPosition { get; set; } = new Dictionary<GameObject, Vector3>();
       
        public float SqrtDistanceToTarget
        {
            get
            {
                if (CurrentEnemy)
                    return (CurrentEnemy.transform.position - transform.position).sqrMagnitude;

                return Mathf.Infinity;
            }
        }
        
        public float DistanceToTarget
        {
            get
            {
                if (CurrentEnemy)
                    return Vector3.Distance(CurrentEnemy.transform.position, transform.position);

                return Mathf.Infinity;
            }
        }

        public Vector3 TargetAimPos
        {
            get
            {
                if (CurrentEnemy)
                    return CurrentEnemy.transform.GetChild(0).position;
                
                return Vector3.zero;
            }
        }
        public Vector3 SelfAimPos => transform.GetChild(0).position;

        public float CoverTimer;
        public float AimTimer;

        private void Start()
        {
            _arsenalView = GetComponent<CharacterView>().ArsenalView;
        }
    }
}