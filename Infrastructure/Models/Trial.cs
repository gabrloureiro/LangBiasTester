using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        // Stimulus properties
        [JsonInclude]
        internal Stimulus Stimulus { get; set; }
        [JsonInclude]
        internal int StimulusNumber { get; set; }

        // Response properties
        [JsonInclude]
        internal Keys? Response { get; set; }
        [JsonInclude]
        internal float ResponseConfidence { get; set; }
        [JsonInclude]
        internal int ResponseTime { get; set; }

        // Trial block
        [JsonInclude]
        internal int Block { get; set; }

        // Constructor
        internal Trial(Stimulus stimulus) => Stimulus = stimulus;
    }
}
