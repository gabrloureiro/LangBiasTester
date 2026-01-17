using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class LangAudio : Audio
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Lang? Lang { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<Lang, int>? MixLangs { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VoiceGender? VoiceGender { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? RawAudios { get; set; }
    }
}
