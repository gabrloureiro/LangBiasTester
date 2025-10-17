using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;
using static LangBiasTester.Infrastructure.Models.Lang;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ConditionManager
    {
        // generates conditions based on explicit ratios given (e.g., 100/0, 75/25, 50/50, 25/75, 0/100)
        internal void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, List<List<int>> explicitRatios)
        {
            var lang1List = audioList.Where(audio => (audio.Lang == lang1)).ToList();
            var lang2List = audioList.Where(audio => (audio.Lang == lang2)).ToList();

            var audioCount = Math.Min(lang1List.Count, lang2List.Count);

            foreach (List<int> ratio in explicitRatios)
            {
                int lang1ct = audioCount*ratio[0] / 100;
                int lang2ct = audioCount*ratio[1] / 100;

                conditions.Add(new Condition(lang1, lang2, lang1ct, lang2ct));
            }
        }
        // TODO (low-priority): generates conditions based on the increment in percentage (e.g., incr=25 will generate the same list as above)
        internal void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, int percentageIncrement)
        {
            // finds number of audios (validates split), converts from %increment to discrete audio increment and calls Init overload underneath
        }
        // TODO (low-priority): generates conditions based on the exact number of audios and a given increment (e.g. (ct=4, incr=1), 4/0, 3/1, 2/2, 1/3, 0/4)
        internal void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, int audioCount, int increment)
        {
            for (int i = 0; i <= audioCount; i += increment)
            {
                conditions.Add(new Condition(lang1, lang2, audioCount-i, i));
            }
        }
        //TODO: split audios evenly according to conditions. Calls ValidateSplit to ensure it can be done.
        internal void EvenSplit() { }

        //TODO: check if audios can be evenly split given the conditions
        internal void ValidateSplit() { }
    }
}
