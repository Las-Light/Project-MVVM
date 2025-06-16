using UnityEngine;

namespace NothingBehind.Scripts.Game.GlobalMap.Logic.Animation
{
    public class GMAnimatorController : MonoBehaviour
    {
        private static readonly int AnimIDSpeed = Animator.StringToHash("Speed");
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Move(float speed)
        {
            _animator.SetFloat(AnimIDSpeed, speed);
        }
    }
}