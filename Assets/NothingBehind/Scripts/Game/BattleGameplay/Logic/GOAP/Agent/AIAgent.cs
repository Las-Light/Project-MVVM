using System.Collections.Generic;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.ActionController;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.Animation;
using NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.GOAP;
using UnityEngine;

namespace NothingBehind.Scripts.Game.BattleGameplay.Logic.GOAP.Agent
{
    public abstract class AIAgent: MonoBehaviour, IGoap
    {
        public float ArrivalDistance = 1f;

        private GoapAgent _goapAgent;
        private AnimatorController _animatorController;
        private EnemyMovementController _controller;

        void Awake()
        {
            _goapAgent = GetComponent<GoapAgent>();
            _animatorController = GetComponent<AnimatorController>();
            _controller = GetComponent<EnemyMovementController>();
        }

        public bool Interrupt { get; set; }
        public abstract HashSet<KeyValuePair<string, object>> GetWorldState();

        public abstract HashSet<KeyValuePair<string, object>> CreateGoalState();


        public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
        {
            // Not handling this here since we are making sure our goals will always succeed.
            // But normally you want to make sure the world state has changed before running
            // the same goal again, or else it will just fail.
        }

        public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
        {
            // Yay we found a plan for our goal
            Debug.Log("<color=green>Plan found</color> " + GoapAgent.PrettyPrint(actions) + gameObject.name);
        }

        public void ActionsFinished()
        {
            // Everything is done, we completed our actions for this gool. Hooray!
            Debug.Log("<color=blue>Actions completed</color> " + gameObject.name);
        }

        public void PlanAborted(GoapAction aborter)
        {
            // An action bailed out of the plan. State has been reset to plan again.
            // Take note of what happened and make sure if you run the same goal again
            // that it can succeed.
            Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.PrettyPrint(aborter) + " " + gameObject.name);
            GetComponent<GoapAgent>().GetDataProvider().ActionsFinished();
            aborter.Reset();
            aborter.DoReset();
        }

        public bool MoveAgent(GoapAction nextAction)
        {
            _animatorController.Grounded(true);
            // move towards the NextAction's target
            if (Interrupt)
            {
                Debug.Log("Interrupt");
                _goapAgent.GetDataProvider().PlanAborted(nextAction);

                Interrupt = false;

                return true;
            }

            if (nextAction.Target == null)
            {
                return false;
            }

            if (nextAction.RequiresInRange())
            {
                _controller.Move(nextAction.Target);
            }

            float targetDistance = (gameObject.transform.position - nextAction.Target.transform.position).magnitude;
            if (targetDistance <= ArrivalDistance)
            {
                // we are at the target location, we are done
                nextAction.SetInRange(true);
                return true;
            }
            else
                return false;
        }
    }
}