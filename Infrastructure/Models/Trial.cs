using System.Text.Json.Serialization;
using LangBiasTester.Infrastructure.Enums;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        // Response properties
        [JsonInclude]
        internal Keys? Response { get; set; }
        [JsonInclude]
        internal float ResponseConfidence { get; set; }
        [JsonInclude]
        internal Lang? ResponseLang { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        internal TimeSpan ResponseTime { get; set; }


        // Trial block and sequence
        [JsonInclude, JsonPropertyName("TrialBlock")]
        internal int Block { get; set; }
        [JsonInclude, JsonPropertyName("SequenceNumber")]
        internal int Sequence { get; set; }


        // Stimulus properties
        [JsonInclude]
        internal int StimulusNumber { get; set; }
        
        [JsonInclude]
        internal Stimulus Stimulus { get; set; }


        // Constructor
        internal Trial(Stimulus stimulus) => Stimulus = stimulus;
    }
}
