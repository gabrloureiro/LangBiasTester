using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;

namespace LangBiasTester.Infrastructure.Models
{
    internal class LangAudio : Audio
    {
        public Lang? Lang { get; set; }
        public Dictionary<Lang, int>? MixLangs { get; set; }
        public VoiceGender? VoiceGender { get; set; }
    }
}
