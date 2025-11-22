using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {   
        internal static void Init(LinkedList<Trial> trials, List<Condition> conditions, List<Audio> mixedAudios, int audioDuration, int seed)
        {
            var conditionSequence = new T1I1(seed, conditions.Count).GenerateSequence();

            for (int i = 0; i < conditionSequence.Count; i++)
            {
                var conditionNumber = conditionSequence[i];

                var newTrial = new Trial(conditions[conditionNumber])
                {
                    MixedAudio = mixedAudios[i] //change later based on how mixedAudios and conditions end up being related
                };

                trials.AddLast(newTrial);
            }
        }

        internal static void Init(LinkedList<Trial> trials, List<Condition> conditions, List<Audio> allAudios, int audioDuration, int trialsPerCondition, int rounds)
        {
            int totalConditions = conditions.Count;

            var audioManager = new AudioManager();

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

            trials.OrderBy(trial => trial.Round); // this is why shuffling isn't working
        }
    }
}
