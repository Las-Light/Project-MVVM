using System;
using NothingBehind.Scripts.Game.GlobalMap.Logic.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace NothingBehind.Scripts.Game.GlobalMap.Logic.ActionController
{
    public class GMPlayerMovementController : MonoBehaviour
    {
        [SerializeField] private float speedBlend = 10f;

        private NavMeshAgent _navMeshAgent;
        private GMAnimatorController _animatorController;
        private float _speedBlend;


        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animatorController = GetComponent<GMAnimatorController>();
        }

        private void Update()
        {
            AnimatedMove(_navMeshAgent.velocity);
        }

        public void MoveToTarget(Vector3 position)
        {
            _navMeshAgent.SetDestination(position);
            //AnimatedMove(_navMeshAgent.velocity);
        }

        private void AnimatedMove(Vector3 desiredVector)
        {
            _speedBlend = Mathf.Lerp(_speedBlend, desiredVector.magnitude, Time.deltaTime * speedBlend);
            _animatorController.Move(_speedBlend);
        }
    }
}