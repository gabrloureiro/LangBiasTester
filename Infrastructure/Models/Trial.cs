using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        [JsonInclude]
        internal Condition Condition { get; set; }
        [JsonInclude]
        internal List<Audio> Audios { get; set; }
        [JsonInclude]
        internal Keys? Response { get; set; }
        [JsonInclude]
        internal Lang? ResponseLang { get; set; }
        [JsonInclude]
        internal int? Round { get; set; }
        
        internal Trial(Condition condition)
        {
            Audios = [];
            Condition = condition;
        }
    }
}
