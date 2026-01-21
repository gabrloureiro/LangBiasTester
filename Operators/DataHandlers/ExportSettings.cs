using LangBiasTester.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace LangBiasTester.Operators.DataHandlers
{
    internal struct ExportSettings
    {
        [JsonConverter(typeof(InlineCollectionConverterFactory))]
        public List<Lang> Langs { get; set; }


        [JsonConverter(typeof(InlineCollectionConverterFactory))]
        public Dictionary<Lang, List<Keys>> LangKeys { get; set; }


        public bool ControlIncluded { get; set; }
        public Lang ControlLang { get; set; }


        [JsonConverter(typeof(InlineCollectionConverterFactory))]
        public int[] StimulusSequence { get; set; }
    }
}
