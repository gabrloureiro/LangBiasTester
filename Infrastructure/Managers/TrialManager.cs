using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class TrialManager
    {
        internal AudioManager audios;

        internal TrialManager()
        {
            audios = new AudioManager();
        }

        internal void Init
            (
            LinkedList<Trial> trials,
            List<Condition> conditions,
            List<Audio> allAudios,
            int trialsPerCondition,
            int rounds,
            int audioDuration
            )
        {
            int totalConditions = conditions.Count;

            // Generate trials one by one, assigning mixed audios, rounds, and conditions
            for (int trialNumber = 0; trialNumber < trialsPerCondition * totalConditions * rounds; trialNumber++)
            {
                int currentConditionIndex = (trialNumber / trialsPerCondition) % totalConditions;
                int currentRound = (trialNumber / (trialsPerCondition * totalConditions));

                Trial newTrial = new(conditions[currentConditionIndex], currentRound)
                {
                    Audios = audios.PrepareMix(allAudios, conditions[currentConditionIndex], audioDuration)
                };

                trials.AddLast(newTrial);
            }

            trials.OrderBy(trial => trial.Round);
        }

        //TODO: implement T1I1 algorithm to shuffle "Trials" IN PLACE (see KW's matlab script)
        internal void T1I1(List<Trial> trials)
        {
            int rounds = trials[^1].Round;

            for (int round = 0; round <= rounds; round++)
            {
                //shuffle filtered list
                //perhaps have a "T1I1" function which just generates the indices -- get the total number of conditions from filtered list
            }
        }
    }
}
