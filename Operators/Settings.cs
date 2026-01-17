using LangBiasTester.Infrastructure.Abstracts;
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
        public int TrainingTrials { get; set; }
        public bool TestReactionTime { get; set; }
        public bool TestConfidence { get; set; }


        public int TimeBetweenTrials { get; set; }
        public int StimulusDuration { get; set; }


        public bool UseT1I1Shuffling { get; set; }
        public int TrialBlocks { get; set; }
        public int SequencesPerTrialBlock { get; set; }


        public List<Lang>? TestLangs { get; set; }
        public bool IncludeControl { get; set; }
        public Lang ControlLang { get; set; }


        public List<List<Keys>>? LangKeys { get; set; }


        public bool ShuffleTextLang { get; set; }
        public bool ShuffleLangDirection { get; set; }


        public int AudioCount { get; set; }
        public bool UseExplicitMorphSteps { get; set; }
        public int MorphCount { get; set; }
        public List<List<int>>? MorphSteps { get; set; }


        public string? RawAudioFilePath { get; set; }
        public bool UseStaticMixes { get; set; }
        public string? MixedAudioFilePath { get; set; }


        public Dictionary<Lang, string>? Instructions { get; set; }


        [JsonIgnore]
        internal Dictionary<Lang, List<Keys>>? LangKeyDictionary { get; set; }

        [JsonIgnore]
        internal Dictionary<Keys, float>? ConfidenceRatings { get; set; }

        [JsonIgnore]
        internal List<Keys>? ValidKeys { get; set; }
        }
}
