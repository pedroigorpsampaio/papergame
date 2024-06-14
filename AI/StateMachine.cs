﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class StateMachine
    {
        public Process GetProcess()
        {
            return new Process();
        }

        public enum ProcessState
        {
            Patrolling,
            Attacking,
            Following,
            Running,
            Terminated
        }

        public enum Command
        {
            Patrol,
            Attack,
            Run,
            Follow,
            Reward
        }

        public class Process
        {
            class StateTransition
            {
                readonly ProcessState CurrentState;
                readonly Command Command;

                public StateTransition(ProcessState currentState, Command command)
                {
                    CurrentState = currentState;
                    Command = command;
                }

                public override int GetHashCode()
                {
                    return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    StateTransition other = obj as StateTransition;
                    return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
                }
            }

            Dictionary<StateTransition, ProcessState> transitions;
            public ProcessState CurrentState { get; private set; }

            public Process()
            {
                CurrentState = ProcessState.Patrolling;
                transitions = new Dictionary<StateTransition, ProcessState>
            {
                { new StateTransition(ProcessState.Patrolling, Command.Reward), ProcessState.Terminated },
                { new StateTransition(ProcessState.Patrolling, Command.Patrol), ProcessState.Patrolling },
                { new StateTransition(ProcessState.Patrolling, Command.Attack), ProcessState.Attacking },
                { new StateTransition(ProcessState.Patrolling, Command.Follow), ProcessState.Following },
                { new StateTransition(ProcessState.Patrolling, Command.Run), ProcessState.Running },
                { new StateTransition(ProcessState.Attacking, Command.Run), ProcessState.Running },
                { new StateTransition(ProcessState.Attacking, Command.Follow), ProcessState.Following },
                { new StateTransition(ProcessState.Attacking, Command.Patrol), ProcessState.Patrolling },
                { new StateTransition(ProcessState.Attacking, Command.Attack), ProcessState.Attacking },
                { new StateTransition(ProcessState.Following, Command.Run), ProcessState.Running },
                { new StateTransition(ProcessState.Following, Command.Follow), ProcessState.Following },
                { new StateTransition(ProcessState.Following, Command.Patrol), ProcessState.Patrolling },
                { new StateTransition(ProcessState.Following, Command.Attack), ProcessState.Attacking },
                { new StateTransition(ProcessState.Running, Command.Run), ProcessState.Running },
                { new StateTransition(ProcessState.Running, Command.Follow), ProcessState.Following },
                { new StateTransition(ProcessState.Running, Command.Patrol), ProcessState.Patrolling },
                { new StateTransition(ProcessState.Running, Command.Attack), ProcessState.Attacking }
            };
            }

            public ProcessState GetNext(Command command)
            {
                StateTransition transition = new StateTransition(CurrentState, command);
                ProcessState nextState;
                if (!transitions.TryGetValue(transition, out nextState))
                    throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
                return nextState;
            }

            public ProcessState MoveNext(Command command)
            {
                CurrentState = GetNext(command);
                return CurrentState;
            }
        }
    }
}
