using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamerator.AI
{
    public class EventsController
    {
        /// <summary>
        /// Tipos de eventos que afetam um objeto IA
        /// </summary>
        public enum Type
        {
            playerAttack,
            playerInSight,
            playerWithinReach,
            playerWithinFollowRange,
            playerFoodOffer,
            playerTreasureOffer,
            playerArmorOffer,
            playerWeaponOffer,
            playerOutOfReach,
            playerOutOfSight,
            lowHealth,
            playerOutOfFollowRange
        };

        /// <summary>
        /// Dispara as consequencias de um evento sobre um objeto IA afetado.
        /// </summary>
        /// <param name="enemy">Objeto IA afetado.</param>
        /// <param name="type">Tipo de evento ocorrido.</param>
        public static void DispatchEvent(Enemy enemy, Type type)
        {
            switch (type)
            {
                case Type.playerAttack:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Rage"));
                    break;
                case Type.playerInSight:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Interest"));
                    break;
                case Type.playerWithinReach:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Vigilance"));
                    break;
                case Type.playerWithinFollowRange:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Antecipation"));
                    break;
                case Type.playerOutOfFollowRange:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Serenity"));
                    break;
                case Type.playerOutOfReach:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Distraction"));
                    break;
                case Type.playerFoodOffer:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Love"));
                    break;
                case Type.playerTreasureOffer:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Submission"));
                    break;
                case Type.playerArmorOffer:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Trust"));
                    break;
                case Type.playerWeaponOffer:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Trust"));
                    break;
                case Type.playerOutOfSight:
                    UpdateEmotion(enemy, Convert_TagToEmotion("Serenity"));
                    break;
                case Type.lowHealth:           
                    UpdateEmotion(enemy, Convert_TagToEmotion("Terror"));
                    break;
                default:
                    Console.WriteLine("Unknown event for AI");
                    break;
            }
        }

        /// <summary>
        /// Atualiza emoçao atual do objeto IA de acordo com o evento ocorrido.
        /// </summary>
        /// <param name="eventTarget">Objeto IA afetado.</param>
        /// <param name="eventValues">Valores do evento.</param>
        private static void UpdateEmotion(Enemy eventTarget, float[] eventValues)
        {
            eventTarget.GetEmotion().currentEmotion =
                eventTarget.GetEmotion().GenerateNewEmotion(
                    eventTarget.GetEmotion().currentEmotion,
                    eventTarget.GetEmotion().enemy_event.GenerateReaction(
                        eventValues,
                        eventTarget.GetPersonality().personality,
                        eventTarget.GetPersonality().PositiveFactors,
                        eventTarget.GetPersonality().NegativeFactors));
        }

        /// <summary>
        /// Atualiza mental state do objeto IA de acordo com o evento ocorrido.
        /// </summary>
        /// <param name="eventTarget">Objeto IA afetado.</param>
        /// <param name="eventValues">Valores do evento.</param>
        public static void UpdateMentalState(Enemy eventTarget, float[] eventValues)
        {
            eventTarget.GetMentalState().CurrentMentalState =
                eventTarget.GetMentalState().UpdateMentalState(
                Convert_EmotionToTag(FindMostInfluentEmotion(eventValues)));
            eventTarget.GetPlan().ClearPlan(eventTarget.GetPlan().plan);
            eventTarget.GetPlan().plan = eventTarget.GetPlan().GeneratePlan(eventTarget.GetMentalState().CurrentMentalState.ToString());

          //  Console.WriteLine(Convert_EmotionToTag(FindMostInfluentEmotion(eventValues)));
        }

        /// <summary>
        /// Recebe um vetor de emocao e retorna um vetor com somente as emocoes influentes, e zera as outras
        /// O valor minimo de disparidade para desconciderar uma emoçao como influente e de .1f para com a maior emoçao
        /// </summary>
        /// <param name="emoAtual">Vetor Emoção Atual</param>
        /// <returns>Vetor com somente as emocoes influentes</returns>
        public static float[] FindMostInfluentEmotion(float[] emoAtual)
        {
            int i;
            int indice = 0;
            float[] aux = new float[4];
            float maiorValor = 0;

            for (i = 0; i < 4; i++)
            {
                aux[i] = emoAtual[i];
                if (Math.Abs(emoAtual[i]) > maiorValor)
                {
                    maiorValor = Math.Abs(emoAtual[i]);
                    indice = i;
                }
            }

            for (i = 0; i < 4; i++)
            {

                if (i != indice)
                {
                    if (i == ((indice + 2) % 4))
                    {
                        aux[i] = 0;
                    }
                }
                if (!(Math.Abs(aux[i]) >= maiorValor - 0.1f))
                {
                    aux[i] = 0;
                }
            }

            return aux;
        }

        private static float[] GetBiggerValue(float[] emoAtual)
        {
            float[] aux = new float[4];
            int i;
            float maiorValor = 0;

            for (i = 0; i < 4; i++)
            {
                aux[i] = emoAtual[i];
                if (Math.Abs(emoAtual[i]) > maiorValor)
                {
                    maiorValor = Math.Abs(emoAtual[i]);
                }
            }

            for (i = 0; i < 4; i++)
            {
                if (!(Math.Abs(aux[i]) == maiorValor))
                {
                    aux[i] = 0;
                }
            }

            return aux;
        }

        private static float[] ZeroSmallerValue(float[] emoAtual)
        {
            float[] aux = new float[4];
            int i;
            float menorValor = 1;
            int indice = 0;

            for (i = 0; i < 4; i++)
            {
                aux[i] = emoAtual[i];
                if (Math.Abs(emoAtual[i]) < menorValor && Math.Abs(emoAtual[i]) != 0)
                {
                    menorValor = Math.Abs(emoAtual[i]);
                    indice = i;
                }
            }

            emoAtual[indice] = 0;

            return emoAtual;
        }

        /// <summary>
        /// Recebe um vetor de emocoes e retorna o string relacionado aquele vetor
        /// </summary>
        /// <param name="emoAtual">Vetor Emocao ja padronizado com a funçao FindMostInfluentEmotion</param>
        /// <returns>String relacionado ao vetor</returns>
        public static string Convert_EmotionToTag(float[] emoAtual)
        {
            int i, contador = 0;

            for (i = 0; i < 4; i++)
            {
                if (emoAtual[i] != 0)
                    contador++;
            }

            if (contador > 2)
                emoAtual = ZeroSmallerValue(emoAtual);

            if (emoAtual[0] != 0 && emoAtual[3] != 0)
            {

                if (emoAtual[0] > 0 && emoAtual[3] > 0)
                    return "Awe";
                else if (emoAtual[0] < 0 && emoAtual[3] < 0)
                    return "Aggressiveness";
            }
            if (emoAtual[0] != 0 && emoAtual[1] != 0)
            {
                if (emoAtual[0] > 0 && emoAtual[1] > 0)
                    return "Submission";
                else if (emoAtual[0] < 0 && emoAtual[1] < 0)
                    return "Contempt";
            }
            if (emoAtual[1] != 0 && emoAtual[2] != 0)
            {
                if (emoAtual[1] > 0 && emoAtual[2] > 0)
                    return "Love";
                else if (emoAtual[1] < 0 && emoAtual[2] < 0)
                    return "Remorse";
            }
            if (emoAtual[2] != 0 && emoAtual[3] != 0)
            {
                if (emoAtual[2] > 0 && emoAtual[3] < 0)
                    return "Optimism";
                else if (emoAtual[2] < 0 && emoAtual[3] > 0)
                    return "Disapproval";
            }

            emoAtual = GetBiggerValue(emoAtual);

            if (emoAtual[0] != 0)
            {
                if (emoAtual[0] <= -0.7)
                    return "Rage";
                else if (emoAtual[0] <= -0.4)
                    return "Anger";
                else if (emoAtual[0] < 0)
                    return "Annoyance";

                else if (emoAtual[0] >= 0.7)
                    return "Terror";
                else if (emoAtual[0] >= 0.4)
                    return "Fear";
                else if (emoAtual[0] > 0)
                    return "Apprehension";
            }
            if (emoAtual[1] != 0)
            {
                if (emoAtual[1] <= -0.7)
                    return "Loathing";
                else if (emoAtual[1] <= -0.4)
                    return "Disgust";
                else if (emoAtual[1] < 0)
                    return "Boredom";

                else if (emoAtual[1] >= 0.7)
                    return "Admiration";
                else if (emoAtual[1] >= 0.4)
                    return "Trust";
                else if (emoAtual[1] > 0)
                    return "Acceptance";
            }
            if (emoAtual[2] != 0)
            {
                if (emoAtual[2] <= -0.7)
                    return "Grief";
                else if (emoAtual[2] <= -0.4)
                    return "Sadness";
                else if (emoAtual[2] < 0)
                    return "Pensiveness";

                else if (emoAtual[2] >= 0.7)
                    return "Ecstasy";
                else if (emoAtual[2] >= 0.4)
                    return "Joy";
                else if (emoAtual[2] > 0)
                    return "Serenity";
            }
            if (emoAtual[3] != 0)
            {
                if (emoAtual[3] <= -0.7)
                    return "Vigilance";
                else if (emoAtual[3] <= -0.4)
                    return "Antecipation";
                else if (emoAtual[3] < 0)
                    return "Intrest";

                else if (emoAtual[3] >= 0.7)
                    return "Amazement";
                else if (emoAtual[3] >= 0.4)
                    return "Surprise";
                else if (emoAtual[3] > 0)
                    return "Distraction";
            }

            return "REACTIONLESS";
        }

        /// <summary>
        /// Recebe um string de emocao e retorna o vetor base(1, 0.5, 0.2) correspondente
        /// </summary>
        /// <param name="emoAtual">String Emocao</param>
        /// <returns>Vetor emocao base correspondente a o string</returns>
        private static float[] Convert_TagToEmotion(string emoAtual)
        {
            switch (emoAtual)
            {
                case "Rage":
                    return new float[4] { -1.0f, 0, 0, 0 };
                case "Anger":
                    return new float[4] { -0.5f, 0, 0, 0 };
                case "Annoyance":
                    return new float[4] { -0.2f, 0, 0, 0 };
                case "Apprehension":
                    return new float[4] { 0.2f, 0, 0, 0 };
                case "Fear":
                    return new float[4] { 0.5f, 0, 0, 0 };
                case "Terror":
                    return new float[4] { 1.0f, 0, 0, 0 };
                case "Loathing":
                    return new float[4] { 0, -1.0f, 0, 0 };
                case "Disgust":
                    return new float[4] { 0, -0.5f, 0, 0 };
                case "Boredom":
                    return new float[4] { 0, -0.2f, 0, 0 };
                case "Acceptance":
                    return new float[4] { 0, 0.2f, 0, 0 };
                case "Trust":
                    return new float[4] { 0, 0.5f, 0, 0 };
                case "Admiration":
                    return new float[4] { 0, 1.0f, 0, 0 };
                case "Grief":
                    return new float[4] { 0, 0, -1.0f, 0 };
                case "Sadness":
                    return new float[4] { 0, 0, -0.5f, 0 };
                case "Pensiveness":
                    return new float[4] { 0, 0, -0.2f, 0 };
                case "Serenity":
                    return new float[4] { 0, 0, 0.2f, 0 };
                case "Joy":
                    return new float[4] { 0, 0, 0.5f, 0 };
                case "Ecstasy":
                    return new float[4] { 0, 0, 1.0f, 0 };
                case "Vigilance":
                    return new float[4] { 0, 0, 0, -1.0f };
                case "Antecipation":
                    return new float[4] { 0, 0, 0, -0.5f };
                case "Interest":
                    return new float[4] { 0, 0, 0, -0.2f };
                case "Distraction":
                    return new float[4] { 0, 0, 0, 0.2f };
                case "Surprise":
                    return new float[4] { 0, 0, 0, 0.5f };
                case "Amazement":
                    return new float[4] { 0, 0, 0, 1.0f };
                case "Submission":
                    return new float[4] { 0.5f, 0.5f, 0, 0 };
                case "Love":
                    return new float[4] { 0, 0.5f, 0.5f, 0 };
                case "Optimism":
                    return new float[4] { 0, 0, 0.5f, -0.5f };
                case "Aggressiveness":
                    return new float[4] { -0.5f, 0, 0, -0.5f };
                case "Contempt":
                    return new float[4] { -0.5f, -0.5f, 0, 0 };
                case "Remorse":
                    return new float[4] { 0, -0.5f, -0.5f, 0 };
                case "Disaproval":
                    return new float[4] { 0, 0, -0.5f, 0.5f };
                case "Awe":
                    return new float[4] { 0.5f, 0, 0, 0.5f };
                default:
                    Console.WriteLine("Unknown emotion");
                    return new float[4] { 0, 0, 0, 0 };
            }
        }

        public static void ZeroEmotionAxis(float[] emoAtual, int axis)
        {
            emoAtual[axis] = 0;
        }
        public static void InvertEmotionAxis(float[] emoAtual, int axis)
        {
            emoAtual[axis] = emoAtual[axis] * (-1);
        }
    }
}
