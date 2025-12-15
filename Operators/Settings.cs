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


        public int TimeBetweenTrials { get; set; }
        public int StimulusDuration { get; set; }


        public bool UseT1I1Shuffling { get; set; }
        public int TrialBlocks { get; set; }
        public int SequencesPerTrialBlock { get; set; }


        public Lang TestLang1 { get; set; }
        public Lang TestLang2 { get; set; }
        public bool IncludeControl { get; set; }
        public Lang ControlLang { get; set; }

        
        public Keys Lang1Direction { get; set; }
        public Keys Lang2Direction { get; set; }


        public bool ShuffleTextLang { get; set; }
        public bool ShuffleLangDirection { get; set; }


        public Dictionary<int, int>? Stimuli { get; set; }
        public string? RawAudioFilePath { get; set; }
        public bool UseStaticMixes { get; set; }
        public string? MixedAudioFilePath { get; set; }


        [JsonIgnore]
        internal Dictionary<Lang, Keys>? LangKeyDictionary { get; set; }
    }
}
