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
        internal static void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, Dictionary<int, int> explicitRatios)
        {
            var lang1List = audioList.Where(audio => (audio.Lang == lang1)).ToList();
            var lang2List = audioList.Where(audio => (audio.Lang == lang2)).ToList();

            var audioCount = Math.Min(lang1List.Count, lang2List.Count);

            foreach (KeyValuePair<int, int> ratio in explicitRatios)
            {
                int lang1ct = audioCount*ratio.Key / 100;
                int lang2ct = audioCount*ratio.Value / 100;

                conditions.Add(new Condition(lang1, lang2, lang1ct, lang2ct));
            }
        }
        // TODO (low-priority): generates conditions based on the increment in percentage (e.g., incr=25 will generate the same list as above)
        internal static void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, int percentageIncrement)
        {
            // finds number of audios (validates split), converts from %increment to discrete audio increment and calls Init overload underneath
        }
        internal static void Init(List<Condition> conditions, List<Audio> audioList, Lang lang1, Lang lang2, int audioCount, int increment)
        {
            for (int i = 0; i <= audioCount; i += increment)
            {
                conditions.Add(new Condition(lang1, lang2, audioCount-i, i));
            }
        }
        //TODO: split audios evenly according to conditions. Calls ValidateSplit to ensure it can be done.
        internal static void EvenSplit() { }

        //TODO: check if audios can be evenly split given the conditions
        internal static void ValidateSplit() { }
    }
}
