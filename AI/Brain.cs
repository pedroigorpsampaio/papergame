using Microsoft.Xna.Framework;
using System;

namespace Gamerator.AI
{


    /// <summary>
    /// class that represents Enemies brains
    /// always checking what is the next action to do
    /// </summary>
    public class Brain
    {
        private StateMachine.Command command;
        private StateMachine fsm;
        private Enemy enemy;
        StateMachine.Process stateProcess;

        public Brain(Enemy enemy)
        {
            this.enemy = enemy;
        }

        public void Initialize()
        {
            // init finite state machine
            fsm = new StateMachine();

            stateProcess = fsm.GetProcess();

            Console.ReadLine();
        }

        public void Update()
        {

        }

        // see what to do next with 
        // current action and state in statemachine
        internal StateMachine.ProcessState GetState()
        {
            return stateProcess.MoveNext(command);
        }

        // update commnad
        internal void UpdateCommand(StateMachine.Command command)
        {
            this.command = command;
        }
    }
}
