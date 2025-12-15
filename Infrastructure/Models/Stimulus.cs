using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Stimulus
    {
        [JsonInclude]
        internal Dictionary<Lang, int> Langs { get; }

        [JsonInclude]
        internal List<Audio> Audios { get; set; }

        [JsonIgnore]
        internal Dictionary<OffsetSampleProvider, WaveOut> WaveStreams { get; set; }


        internal Stimulus(Lang lang1, Lang lang2, int lang1ct, int lang2ct)
        {
            Langs = [];
            Audios = [];
            WaveStreams = [];
            Langs.Add(lang1, lang1ct);
            Langs.Add(lang2, lang2ct);
        }

        internal Stimulus(Dictionary<Lang, int> langs)
        {
            Langs = langs;
            Audios = [];
            WaveStreams = [];
        }
    }
}
