using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class Plan
    {

        public enum Actions { EvaluateTreat, Atack, Hide, Follow, Cry, Run, Antagonise, LostControl, FindTarget, IDLE, NULL };
        public ArrayList plan;

        // Use this for initialization
        public void Initialize()
        {
            plan = new ArrayList();
        }

        // Update is called once per frame
        public void Update()
        {

        }

        public ArrayList GeneratePlan(string mental_state)
        {
            ArrayList ActionsSequence = new ArrayList();

            if (mental_state == "Scared")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.Run);
                ActionsSequence.Add(Actions.Hide);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Angry")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.FindTarget);
                ActionsSequence.Add(Actions.Atack);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Sad")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.Hide);
                ActionsSequence.Add(Actions.Cry);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Confident")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Repulse")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.Antagonise);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Ready")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.FindTarget);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Happy")
            {
                ActionsSequence.Add(Actions.EvaluateTreat);
                ActionsSequence.Add(Actions.FindTarget);
                ActionsSequence.Add(Actions.Follow);
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Surprised")
            {
                ActionsSequence.Add(Actions.IDLE);
            }
            if (mental_state == "Confused")
            {
                ActionsSequence.Add(Actions.LostControl);
                ActionsSequence.Add(Actions.IDLE);
            }

            return ActionsSequence;
        }

        public void ClearPlan(ArrayList plan)
        {
            plan.Clear();
        }
    }
}
