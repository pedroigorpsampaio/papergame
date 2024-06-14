using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class Emotion
    {

        int i = 0;
        int j = 0;
        public float[] currentEmotion = new float[4];

        public float[] bios = new float[4];
        //[Range(-1f, 1f)]
        public float AngerXFear;
        // [Range(-1f, 1f)]
        public float DisgustXTrust;
        // [Range(-1f, 1f)]
        public float SadnessXJoy;
        //[Range(-1f, 1f)]
        public float AntecipationXSurprise;
        public float[] bioTendency = new float[4];
       
        public Event enemy_event;
        public float BioTendencyTime = 0f;

        private float time;
        private float startTime;

        // Use this for initialization
        public void Initialize(Event enemy_event)
        {
            this.enemy_event = enemy_event;
            bios[0] = AngerXFear;
            bios[1] = DisgustXTrust;
            bios[2] = SadnessXJoy;
            bios[3] = AntecipationXSurprise;

            for (i = 0; i < 4; i++)
            {
                currentEmotion[i] = bios[i];
            }
        }

        // Update is called once per frame
        public void Update(float delta)
        {
            if (CheckBios(currentEmotion, bios))
                time += delta;
            else
                time = 0;

            //float distCovered = (Time.time - startTime) * bioTendency[i];
            //float fracJourney = distCovered / bios[i];

            if (time >= BioTendencyTime)
            {
                time = 0;
                for (i = 0; i < 4; i++)
                {
                    ;
                    if (currentEmotion[i] < bios[i])
                    {
                        if (currentEmotion[i] + bioTendency[i] >= bios[i])
                            currentEmotion[i] = bios[i];
                        else
                            currentEmotion[i] += bioTendency[i];
                    }
                    if (currentEmotion[i] > bios[i])
                    {
                        if (currentEmotion[i] - bioTendency[i] <= bios[i])
                            currentEmotion[i] = bios[i];
                        else
                            currentEmotion[i] -= bioTendency[i];
                    }
                }
            }

            //clamp
            for (j = 0; j < 4; j++)
            {
                if (currentEmotion[j] >= 1f)
                    currentEmotion[j] = 1f;
                else if (currentEmotion[j] <= -1f)
                    currentEmotion[j] = -1f;
            }

            //Linha a ser chamada com novo evento : currentEmotion = GenerateNewEmotion(currentEmotion,events.GenerateReaction(events.InfluentEmotion, events.personality.personality, events.personality.fatores));
        }

        public float[] GenerateNewEmotion(float[] currentEmotion, float[] InfluentEmotion)
        {
            float[] newEmotion = new float[4];
            int i;
            for (i = 0; i < 4; i++)
                newEmotion[i] = currentEmotion[i] + InfluentEmotion[i];

            return newEmotion;
        }

        private static bool CheckBios(float[] currentEmotion, float[] bios)
        {
            for (int i = 0; i > 4; i++)
            {
                if (currentEmotion[i] != bios[i])
                    return false;
            }
            return true;
        }
    }
}
