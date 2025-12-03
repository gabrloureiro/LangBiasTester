using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class ParticipantInfo
    {
        [JsonInclude]
        internal int ID { get; set; }
        internal Lang PreferredLang { get; set; } // should be modified once preferred language criteria is established (as per Dr. Titone's suggestions)
    }
}
