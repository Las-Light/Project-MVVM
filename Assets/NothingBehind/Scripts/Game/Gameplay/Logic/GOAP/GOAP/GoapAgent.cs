using System;
using System.Collections.Generic;
using UnityEngine;

namespace NothingBehind.Scripts.Game.Gameplay.Logic.GOAP.GOAP
{
    public class GoapAgent: MonoBehaviour
    {
        [SerializeField] private GoapAction _currentAction;
        private FSM.FSM _stateMachine;

        private FSM.FSM.FSMState _idleState; // finds something to do
        private FSM.FSM.FSMState _moveToState; // moves to a target
        private FSM.FSM.FSMState _performActionState; // performs an action

        private HashSet<GoapAction> _availableActions;
        private Queue<GoapAction> _currentActions;

        private IGoap
            _dataProvider; // this is the implementing class that provides our world data and listens to feedback on planning

        private GoapPlanner _planner;


        void Start()
        {
            _stateMachine = new FSM.FSM();
            _availableActions = new HashSet<GoapAction>();
            _currentActions = new Queue<GoapAction>();
            _planner = new GoapPlanner();
            FindDataProvider();
            CreateIdleState();
            CreateMoveToState();
            CreatePerformActionState();
            _stateMachine.PushState(_idleState);
            LoadActions();
        }


        void Update()
        {
            _stateMachine.Update(this.gameObject);
        }


        public void AddAction(GoapAction a)
        {
            _availableActions.Add(a);
        }

        public GoapAction GetAction(Type action)
        {
            foreach (GoapAction g in _availableActions)
            {
                if (g.GetType().Equals(action))
                    return g;
            }

            return null;
        }

        public void RemoveAction(GoapAction action)
        {
            _availableActions.Remove(action);
        }

        private bool HasActionPlan()
        {
            return _currentActions.Count > 0;
        }

        private void CreateIdleState()
        {
            _idleState = (fsm, gameObj) =>
            {
                // GOAP planning

                // get the world state and the goal we want to plan for
                HashSet<KeyValuePair<string, object>> worldState = _dataProvider.GetWorldState();
                HashSet<KeyValuePair<string, object>> goal = _dataProvider.CreateGoalState();

                // Plan
                Queue<GoapAction> plan = _planner.Plan(gameObject, _availableActions, worldState, goal);
                if (plan != null)
                {
                    // we have a plan, hooray!
                    _currentActions = plan;
                    _dataProvider.PlanFound(goal, plan);

                    fsm.PopState(); // move to PerformAction state
                    fsm.PushState(_performActionState);
                }
                else
                {
                    // ugh, we couldn't get a plan
                    Debug.Log("<color=orange>Failed Plan:</color>" + PrettyPrint(goal) + " " + gameObject.name);
                    _dataProvider.PlanFailed(goal);
                    fsm.PopState(); // move back to IdleAction state
                    fsm.PushState(_idleState);
                }
            };
        }

        private void CreateMoveToState()
        {
            _moveToState = (fsm, gameObj) =>
            {
                // move the game object

                GoapAction action = _currentActions.Peek();
                if (action.RequiresInRange() && action.Target == null)
                {
                    Debug.Log(
                        "<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");
                    fsm.PopState(); // move
                    fsm.PopState(); // perform
                    fsm.PushState(_idleState);
                    return;
                }

                // get the agent to move itself
                if (_dataProvider.MoveAgent(action))
                {
                    fsm.PopState();
                }
            };
        }

        private void CreatePerformActionState()
        {
            _performActionState = (fsm, gameObj) =>
            {
                // perform the action

                if (!HasActionPlan())
                {
                    // no actions to perform
                    Debug.Log("<color=red>Done actions</color>");
                    fsm.PopState();
                    fsm.PushState(_idleState);
                    _dataProvider.ActionsFinished();
                    return;
                }

                GoapAction action = _currentActions.Peek();
                if (action.IsDone())
                {
                    // the action is done. Remove it so we can perform the next one
                    _currentActions.Dequeue();
                }

                if (HasActionPlan())
                {
                    // perform the next action
                    action = _currentActions.Peek();
                    _currentAction = action;
                    bool inRange = action.RequiresInRange() ? action.IsInRange() : true;

                    if (inRange)
                    {
                        // we are in range, so perform the action
                        bool success = action.Perform(gameObj);

                        if (!success)
                        {
                            // action failed, we need to plan again
                            fsm.PopState();
                            fsm.PushState(_idleState);
                            _dataProvider.PlanAborted(action);
                        }
                    }
                    else
                    {
                        // we need to move there first
                        // push moveTo state
                        fsm.PushState(_moveToState);
                    }
                }
                else
                {
                    // no actions left, move to Plan state
                    fsm.PopState();
                    fsm.PushState(_idleState);
                    _dataProvider.ActionsFinished();
                }
            };
        }

        private void FindDataProvider()
        {
            foreach (UnityEngine.Component comp in gameObject.GetComponents(typeof(UnityEngine.Component)))
            {
                if (typeof(IGoap).IsAssignableFrom(comp.GetType()))
                {
                    _dataProvider = (IGoap)comp;
                    return;
                }
            }
        }

        private void LoadActions()
        {
            GoapAction[] actions = gameObject.GetComponents<GoapAction>();
            foreach (GoapAction a in actions)
            {
                _availableActions.Add(a);
            }

            Debug.Log("Found actions: " + PrettyPrint(actions));
        }

        public static string PrettyPrint(HashSet<KeyValuePair<string, object>> state)
        {
            String s = "";
            foreach (KeyValuePair<string, object> kvp in state)
            {
                s += kvp.Key + ":" + kvp.Value.ToString();
                s += ", ";
            }

            return s;
        }

        public static string PrettyPrint(Queue<GoapAction> actions)
        {
            String s = "";
            foreach (GoapAction a in actions)
            {
                s += a.GetType().Name;
                s += "-> ";
            }

            s += "GOAL";
            return s;
        }

        public static string PrettyPrint(GoapAction[] actions)
        {
            String s = "";
            foreach (GoapAction a in actions)
            {
                s += a.GetType().Name;
                s += ", ";
            }

            return s;
        }

        public static string PrettyPrint(GoapAction action)
        {
            String s = "" + action.GetType().Name;
            return s;
        }

        public IGoap GetDataProvider()
        {
            return _dataProvider;
        }
    }
}