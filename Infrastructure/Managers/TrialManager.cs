using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {   
        internal static void Init(LinkedList<Trial> trials, List<Condition> conditions, List<Audio> mixedAudios, int seed, int audioDuration)
        {
            var t1i1 = new T1I1(seed, conditions.Count);
            
            var trialSequence = t1i1.GenerateSequence();

            for (int i = 0; i < trialSequence.Count; i++)
            {
                var conditionIndex = trialSequence[i] - 1;

                var newTrial = new Trial(conditions[conditionIndex])
                {
                    Condition = conditions[conditionIndex],
                    Round = i / conditions.Count
                };

                newTrial.Audios.Add(mixedAudios[conditionIndex]);
                newTrial.Audios[0].SnippetDuration = audioDuration;

                trials.AddLast(newTrial);
            }
        }

        internal static void Init(LinkedList<Trial> trials, List<Condition> conditions, List<Audio> allAudios, int seed, int audioDuration, int trialsPerCondition, int rounds)
        {
            int totalConditions = conditions.Count;

            var audioManager = new AudioManager(seed);

            // Generate trials one by one, assigning mixed audios, rounds, and conditions
            for (int trialNumber = 0; trialNumber < trialsPerCondition * totalConditions * rounds; trialNumber++)
            {
                int currentConditionIndex = (trialNumber / trialsPerCondition) % totalConditions;
                int currentRound = (trialNumber / (trialsPerCondition * totalConditions));

                Trial newTrial = new(conditions[currentConditionIndex])
                {
                    Round = currentRound,
                    Audios = audioManager.PrepareMix(allAudios, conditions[currentConditionIndex], audioDuration)
                };

                trials.AddLast(newTrial);
            }

            trials.OrderBy(trial => trial.Round);
        }
    }
}
