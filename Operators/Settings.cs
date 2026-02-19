using LangBiasTester.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace LangBiasTester.Operators
{
    internal class Settings
    {
        public int AdminID { get; set; }
        public bool AnonymizeParticipantID { get; set; }

        public bool RunDemo { get; set; }


        public bool RunTraining { get; set; }
        public Keys? RepeatTrainingKey { get; set; }


        public bool TestReactionTime { get; set; }
        public bool TestConfidence { get; set; }


        public int TimeBetweenTrials { get; set; }
        public int StimulusDuration { get; set; }


        public bool UseT1I1Shuffling { get; set; }
        public int TrialBlocks { get; set; }
        public int SequencesPerTrialBlock { get; set; }

        public bool IncludeControl { get; set; }

        public string? OutputFolder { get; set; }
        
        [JsonIgnore]
        internal Dictionary<Keys, float>? ConfidenceRatings { get; set; }

        [JsonIgnore]
        internal List<Keys>? ValidKeys { get; set; }
    }
}
