using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatrolEnemy
{
    [CreateAssetMenu(menuName = "PatrolEnemy/State")]
    public class State : FSMState
    {
        public List<FSMAction> EnterActions = new List<FSMAction>();
        public List<FSMAction> Action = new List<FSMAction>();
        public List<FSMAction> ExitActions = new List<FSMAction>();
        public List<Transition> Transitions = new List<Transition>();
        public string stateName;

        public override void Execute(FSM stateMachine)
        {
            if(stateMachine.ExecutionReady)
            // execute actions
                foreach (var action in Action)
                    action.Execute(stateMachine);

            if(stateMachine.TransitionReady)
                // try to move to a new state
                foreach(var transition in Transitions)
                {
                    FSMState state = transition.NewState(stateMachine);
                    if(!(state is RemainInState))
                    {
                        stateMachine.ChangeState( state );
                        break; // stop checking for new states
                    }
                }
        }

        public override void Enter(FSM stateMachine)
        {
            if(GeneralFunctions.IsDebug()) Debug.Log("Entering state " + stateName);

            foreach (var action in EnterActions)
                action.Execute(stateMachine);
        }

        public override void Exit(FSM stateMachine)
        {
            if(GeneralFunctions.IsDebug()) Debug.Log("Exiting state " + stateName);

            foreach (var action in ExitActions)
                action.Execute(stateMachine);
        }
    }
}