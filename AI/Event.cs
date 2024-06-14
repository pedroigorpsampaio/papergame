using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class Event
    {
        private float[] influentEmotion = new float[4];

        // Use this for initialization
        void Initialize()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public float[] GenerateReaction(float[] infEmo, float[] perso, float[,] factors, float[,] factors2)
        {
            int i; int j;
            float[] newEmotion = new float[4];

            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 5; j++)
                {
                    if (infEmo[i] > 0)
                        newEmotion[i] += infEmo[i] * perso[j] * factors[j, i];
                    else
                        newEmotion[i] += infEmo[i] * perso[j] * factors2[j, i];
                }
                newEmotion[i] = newEmotion[i] / 5;
            }

            return newEmotion;
        }
    }
}
