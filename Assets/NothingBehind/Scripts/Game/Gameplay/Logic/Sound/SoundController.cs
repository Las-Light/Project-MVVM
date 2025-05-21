using NothingBehind.Scripts.Game.Gameplay.Logic.Data;
using NothingBehind.Scripts.Game.Gameplay.Logic.SensorySystem;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.Sound
{
    public class SoundController : MonoBehaviour
    {
        [Space(10)] [Header("Footstep audio")] public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] [SerializeField] private float FootstepAudioVolume = 0.5f;

        [Space(10)] [Header("Make sound properties")] [SerializeField]
        private float SoundRadius = 25f;

        [SerializeField] private LayerMask SoundMask;
        [SerializeField] private LayerMask ObstacleMask;

        //метод проигрывает звук выстрела и доносит его до тех кто его услышал в прямом радиусе. Цель которая
        //его услышала начинает расследование места выстрела
        
        //TODO: uncommit after added Hearing and SoundType 
        public void MakeSoundSelf(Vector3 soundPos, SoundType soundType)
        {
            Collider[] listenerInSoundRadius = new Collider[50];
        
            int size = Physics.OverlapSphereNonAlloc(soundPos, SoundRadius, listenerInSoundRadius, SoundMask);
        
            for (int i = 0; i < size; i++)
            {
                if (listenerInSoundRadius[i].gameObject == gameObject)
                    continue;
        
                listenerInSoundRadius[i].gameObject.TryGetComponent(out Hearing listener);
        
                if (!listener)
                    continue;
        
                Vector3 listenerPos = listener.transform.GetChild(0).position;
                Vector3 dirToListener = (listenerPos - soundPos).normalized;
                float distanceToListener = Vector3.Distance(soundPos, listenerPos);
                Vector3 soundPosForSearch = new Vector3(soundPos.x, 0, soundPos.z);
        
                if (!Physics.Raycast(soundPos, dirToListener, distanceToListener,
                        ObstacleMask))
                {
                    listener.InvestigateShootingArea(soundPosForSearch, soundType);
                }
            }
        }
        
        public void MakeSoundNotify(Vector3 soundPos, Vector3 targetPos, SoundType soundType)
        {
            Collider[] listenerInSoundRadius = new Collider[50];
        
            int size = Physics.OverlapSphereNonAlloc(soundPos, SoundRadius, listenerInSoundRadius, SoundMask);
        
            for (int i = 0; i < size; i++)
            {
                if (listenerInSoundRadius[i].gameObject == gameObject)
                    continue;
        
                listenerInSoundRadius[i].gameObject.TryGetComponent(out Hearing listener);
        
                if (!listener)
                    continue;
                
                Vector3 listenerPos = listener.transform.GetChild(0).position;
                Vector3 dirToListener = (listenerPos - soundPos).normalized;
                float distanceToListener = Vector3.Distance(soundPos,
                        listenerPos);
        
                if (!Physics.Raycast(soundPos, dirToListener, distanceToListener,
                        ObstacleMask))
                {
                    listener.InvestigateShootingArea(targetPos, soundType);
                }
            }
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position,
                        FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position,
                    FootstepAudioVolume);
            }
        }
    }
}