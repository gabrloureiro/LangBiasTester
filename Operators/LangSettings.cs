using LangBiasTester.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace LangBiasTester.Operators
{
    internal class LangSettings : Settings
    {
        public List<Lang>? TestLangs { get; set; }
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
        public bool RandomizeSnippetStartingPoint { get; set; }
        public bool RestrictSnippetRangePerSequence { get; set; }
        public double AudioMargin { get; set; }


        public Dictionary<Lang, string>? Instructions { get; set; }


        [JsonIgnore]
        internal Dictionary<Lang, List<Keys>>? LangKeyDictionary { get; set; }
    }
}
