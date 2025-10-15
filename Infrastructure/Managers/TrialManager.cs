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

        internal void InitializeTrials
            (
            LinkedList<Trial> trials,
            List<Condition> conditions,
            int trialsPerCondition,
            int rounds,
            int audioDuration
            )
        {
            int round = -1;

            // Generate trials one by one, assigning mixed audios and rounds
            // FIX: rounds are not getting calculated properly
            for (int trialNumber = 0; trialNumber < trialsPerCondition * rounds; trialNumber++)
            {
                int totalConditions = conditions.Count;
                int currentConditionIndex = trialNumber % totalConditions;

                if (currentConditionIndex == 0) round++;

                Trial newTrial = new(conditions[currentConditionIndex], round)
                {
                    Audios = audios.PrepareMix(conditions[currentConditionIndex], audioDuration)
                };

                trials.AddLast(newTrial);
            }
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

        internal void StartTrial(Trial trial)
        {
            audios.Play(trial.Audios);
        }
    }
}
