using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class AudioManager
    {
        internal List<Audio> PrepareMix(List<Audio> audios, Condition condition, int snippetDuration)
        {
            var selectedAudios = SelectAudiosMeetingCondition(audios, condition);
            TrimAudioSnippets(selectedAudios, snippetDuration);
            SelectRandomTimestamp(selectedAudios, snippetDuration);

            return selectedAudios;
        }
        private List<Audio> SelectAudiosMeetingCondition(List<Audio> audios, Condition condition)
        {
            List<Audio> selectedAudios = [];

            foreach (KeyValuePair<Lang, int> langEntry in condition.Langs)
            {
                var filteredAudios = audios.Where(audio => audio.Lang == langEntry.Key).Take(langEntry.Value).ToList(); //randomize (low-priority)
                selectedAudios.AddRange(filteredAudios);
            }

            return selectedAudios;
        }
        
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
            var random = new Random();

            foreach (Audio audio in audios)
            {
                var duration = audio.Duration.TotalSeconds;
                var maxStart = duration - snippetDuration;

                audio.SnippetTimestamp = random.Next(0, (int)maxStart); 
            }
        }
    }
}
