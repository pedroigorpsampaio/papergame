using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class MentalState
    {

        public enum mentalState { Scared, Angry, Sad, Happy, Surprised, Confused, Repulse, Confident, Ready, NULL };
        public mentalState CurrentMentalState = mentalState.NULL;
        private Enemy enemy;

        // Use this for initialization
        public void Initialize(Enemy enemy)
        {
            this.enemy = enemy;
        }

        // Update is called once per frame
        public void Update()
        {
            EventsController.UpdateMentalState(enemy, enemy.GetEmotion().currentEmotion);

            switch (CurrentMentalState)
            {
                case mentalState.Angry:
                    AngryBehavior(enemy);
                    break;
                case mentalState.Confident:
                    ConfidentBehavior(enemy);
                    break;
                case mentalState.Confused:
                    ConfusedBehavior(enemy);
                    break;
                case mentalState.Happy:
                    HappyBehavior(enemy);
                    break;
                case mentalState.Ready:
                    ReadyBehavior(enemy);
                    break;
                case mentalState.Repulse:
                    RepulseBehavior(enemy);
                    break;
                case mentalState.Sad:
                    SadBehavior(enemy);
                    break;
                case mentalState.Scared:
                    ScaredBehavior(enemy);
                    break;
                case mentalState.Surprised:
                    SurprisedBehavior(enemy);
                    break;
            }
        }

        public mentalState UpdateMentalState(string currentEmotion)
        {
            mentalState ment_state = CurrentMentalState;

            switch (currentEmotion)
            {
                case "Rage":
                    ment_state = mentalState.Angry;
                    break;
                case "Anger":
                    ment_state = mentalState.Angry;
                    break;
                case "Annoyance":
                    ment_state = mentalState.Angry;
                    break;
                case "Apprehension":
                    ment_state = mentalState.Scared;
                    break;
                case "Fear":
                    ment_state = mentalState.Scared;
                    break;
                case "Terror":
                    ment_state = mentalState.Scared;
                    break;
                case "Loathing":
                    ment_state = mentalState.Repulse;
                    break;
                case "Disgust":
                    ment_state = mentalState.Repulse;
                    break;
                case "Boredom":
                    ment_state = mentalState.Repulse;
                    break;
                case "Acceptance":
                    ment_state = mentalState.Confident;
                    break;
                case "Trust":
                    ment_state = mentalState.Confident;
                    break;
                case "Admiration":
                    ment_state = mentalState.Confident;
                    break;
                case "Grief":
                    ment_state = mentalState.Sad;
                    break;
                case "Sadness":
                    ment_state = mentalState.Sad;
                    break;
                case "Pensiveness":
                    ment_state = mentalState.Sad;
                    break;
                case "Serenity":
                    ment_state = mentalState.Happy;
                    break;
                case "Joy":
                    ment_state = mentalState.Happy;
                    break;
                case "Ecstasy":
                    ment_state = mentalState.Happy;
                    break;
                case "Vigilance":
                    ment_state = mentalState.Ready;
                    break;
                case "Antecipation":
                    ment_state = mentalState.Ready;
                    break;
                case "Intrest":
                    ment_state = mentalState.Ready;
                    break;
                case "Distraction":
                    ment_state = mentalState.Surprised;
                    break;
                case "Surprise":
                    ment_state = mentalState.Surprised;
                    break;
                case "Amazement":
                    ment_state = mentalState.Surprised;
                    break;
                case "Submission":
                    ment_state = mentalState.Scared;
                    break;
                case "Love":
                    ment_state = mentalState.Happy;
                    break;
                case "Optimism":
                    ment_state = mentalState.Confident;
                    break;
                case "Aggressiveness":
                    ment_state = mentalState.Angry;
                    break;
                case "Contempt":
                    ment_state = mentalState.Repulse;
                    break;
                case "Remorse":
                    ment_state = mentalState.Sad;
                    break;
                case "Disaproval":
                    ment_state = mentalState.Angry;
                    break;
                case "Awe":
                    ment_state = mentalState.Surprised;
                    break;
                default:
                    ment_state = mentalState.Confused;
                    break;
            }

            return ment_state;
        }

        //Para Determinar as reaçoes baseadas em mental states
        private void AngryBehavior(Enemy target)
        {
            if (!enemy.in_follow_range)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
            else if(!enemy.in_reach)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Follow);
            else
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Attack);
        }
        private void ConfidentBehavior(Enemy target)
        {
            if (enemy.in_follow_range && !enemy.in_reach)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Follow);
            else
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void ConfusedBehavior(Enemy target)
        {
            if (enemy.GetCurrentState() != StateMachine.ProcessState.Patrolling)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void SadBehavior(Enemy target)
        {
            if (enemy.GetCurrentState() != StateMachine.ProcessState.Patrolling)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void HappyBehavior(Enemy target)
        {
            if (enemy.GetCurrentState() != StateMachine.ProcessState.Patrolling)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void ReadyBehavior(Enemy target)
        {
            if (enemy.in_follow_range && !enemy.in_reach)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Follow);
            else
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void RepulseBehavior(Enemy target)
        {
            if (enemy.GetCurrentState() != StateMachine.ProcessState.Running &&
                enemy.GetCurrentState() != StateMachine.ProcessState.Patrolling)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Run);
        }
        private void ScaredBehavior(Enemy target)
        {
            // run if scared (until follow range)
            if (enemy.in_follow_range)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Run);
            else
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
        private void SurprisedBehavior(Enemy target)
        {
            if (enemy.GetCurrentState() != StateMachine.ProcessState.Patrolling)
                enemy.GetBrain().UpdateCommand(StateMachine.Command.Patrol);
        }
    }
}
