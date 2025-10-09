using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class AudioManager
    {
        internal List<Audio> PrepareMix(Condition condition, int snippetDuration)
        {
            List<Audio> selectedAudios = SelectAudiosMeetingCondition(condition);
            TrimAudioSnippets(selectedAudios, snippetDuration);
            SelectRandomTimestamp(selectedAudios, snippetDuration);

            return selectedAudios;
        }

        //TODO: filters JSON for "langct" entries of the corresponding "lang" (both in the Condition.langs dictionary),
        //adds each one to the "audios" list, setting FileName, TotalDuration, and Lang.
        //Low-priority: make audio choice random
        private List<Audio> SelectAudiosMeetingCondition(Condition condition) { return []; }
        
        private void TrimAudioSnippets(List<Audio> audios, int snippetDuration)
        {
            foreach (Audio audio in audios)
            {
                audio.SnippetDuration = snippetDuration;
            }
        }

        //TODO: selects random timestamp in each audio and registers it in its "timestamp" property.
        //Uses snippetDuration to determine maximum starting point.
        private void SelectRandomTimestamp(List<Audio> audios, int snippetDuration)
        {
            foreach (Audio audio in audios)
            {
                audio.SnippetTimestamp = 0; //change later so it's random (choose random from 0 to (duration - snippetduration))
            }
        }

        //TODO: finds all audios in "Audios" in the folder through their "FileName"s and plays them all simultaneously,
        //starting from their Timestamp, for the duration specified by their SnippetDuration.
        internal void Play(List<Audio> audios)
        {
            // For testing: display a message
            MessageBox.Show("Test👍");
        }
    }
}
