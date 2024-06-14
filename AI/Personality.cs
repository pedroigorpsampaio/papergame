using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class Personality
    {
        static int qtdEmocoes = 4;
        static int qtdPersonalidades = 5;
        // [Range(0f, 1f)]!
        public float Openness;
        public float Conscientiousness;
        public float Extraversion;
        public float Agreeableness;
        public float Neuroticism;
        // /[Range(0f, 1f)]!
        public float[] personality = new float[qtdPersonalidades];

        public float[,] PositiveFactors = new float[5, 4] {
        { -1, 1, 1, -1 },
        { 0, 1, 0, 0 },
        { -1, 1, 1, 1 },
        { 0, 0, 1, 1 },
        { 1, -1, -1, 1 }
    };

        public float[,] NegativeFactors = new float[5, 4]{
        { -1, -1, -1, 1 },
        { 0, 0, 1, 1 },
        { 0, 0, 1, -1 },
        { 0, -1, 0, 0 },
        { 1, 1, 1, -1 }
       };

        public Personality(float Openness, float Conscientiousness, float Extraversion, float Agreeableness, float Neuroticism)
        {
            this.Openness = Openness;
            this.Conscientiousness = Conscientiousness;
            this.Extraversion = Extraversion;
            this.Agreeableness = Agreeableness;
            this.Neuroticism = Neuroticism;

            MathHelper.Clamp(this.Openness, 0f, 1f);
            MathHelper.Clamp(this.Conscientiousness, 0f, 1f);
            MathHelper.Clamp(this.Extraversion, 0f, 1f);
            MathHelper.Clamp(this.Agreeableness, 0f, 1f);
            MathHelper.Clamp(this.Neuroticism, 0f, 1f);

            Initialize();
        }

        public Personality(float[] personality)
        {
            Openness = personality[0];
            Conscientiousness = personality[1];
            Extraversion = personality[2];
            Agreeableness = personality[3];
            Neuroticism = personality[4];

            MathHelper.Clamp(Openness, 0f, 1f);
            MathHelper.Clamp(Conscientiousness, 0f, 1f);
            MathHelper.Clamp(Extraversion, 0f, 1f);
            MathHelper.Clamp(Agreeableness, 0f, 1f);
            MathHelper.Clamp(Neuroticism, 0f, 1f);

            Initialize();
        }

        // Use this for initialization
        public void Initialize()
        {
            personality[0] = Openness;
            personality[1] = Conscientiousness;
            personality[2] = Extraversion;
            personality[3] = Agreeableness;
            personality[4] = Neuroticism;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
