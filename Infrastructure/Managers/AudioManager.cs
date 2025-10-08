using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class AudioManager
    {
        internal List<Audio> Audios;

        internal AudioManager()
        {
            Audios = [];
        }

        internal void PrepareMix(Condition condition, int snippetDuration)
        {
            SelectAudiosMeetingCondition(condition);
            TrimAudioSnippets(snippetDuration);
            SelectRandomTimestamp(snippetDuration);
        }

        //TODO: filters JSON for "langct" entries of the corresponding "lang" (both in the Condition.langs dictionary),
        //adds each one to the "audios" list, setting FileName, TotalDuration, and Lang.
        //Low-priority: make audio choice random
        internal void SelectAudiosMeetingCondition(Condition condition) { }
        
        internal void TrimAudioSnippets(int snippetDuration)
        {
            foreach (Audio audio in Audios)
            {
                audio.SnippetDuration = snippetDuration;
            }
        }

        //TODO: selects random timestamp in each audio and registers it in its "timestamp" property.
        //Uses snippetDuration to determine maximum starting point.
        internal void SelectRandomTimestamp(int snippetDuration)
        {
            foreach (Audio audio in this.Audios)
            {

            }
        }

        //TODO: finds all audios in "Audios" in the folder through their "FileName"s and plays them all simultaneously,
        //starting from their Timestamp, for the duration specified by their SnippetDuration.
        internal void Play() { }
    }
}
