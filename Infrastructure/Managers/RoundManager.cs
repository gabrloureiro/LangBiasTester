using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class RoundManager
    {
        internal List<Round> Rounds;

        internal RoundManager()
        {
            Rounds = [];
        }
        
        internal void Initialize(List<Condition> conditions, int audioDuration)
        {
            foreach (Round round in Rounds)
            {
                round.Trials.Initialize(conditions, audioDuration);
            }
        }

        internal void Run(int timeBetweenTrials, int responseWindow)
        {
            foreach (Round round in Rounds)
            {
                round.Trials.Run(timeBetweenTrials, responseWindow);
                //Add a break between trials with a rest message for all before the last and a thanking message for the last one.
                //Make it so that break is skipped once participant gives an input.
            }
        }
    }
}
