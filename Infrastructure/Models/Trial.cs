using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        [JsonInclude]
        internal Stimulus Stimulus { get; set; }
        [JsonInclude]
        internal int StimulusNumber { get; set; }
        [JsonInclude]
        internal List<Audio> Audios { get; set; }
        [JsonInclude]
        internal Keys? Response { get; set; }
        [JsonInclude]
        internal Lang? ResponseLang { get; set; }
        [JsonInclude]
        internal int Block { get; set; }
        [JsonInclude]
        internal int ResponseTime { get; set; }

        internal Trial(Stimulus stimulus)
        {
            Audios = [];
            Stimulus = stimulus;
        }
    }
}
