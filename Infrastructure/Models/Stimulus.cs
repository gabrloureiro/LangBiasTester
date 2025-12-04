using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Stimulus
    {
        [JsonInclude]
        internal Dictionary<Lang, int> Langs { get; }

        internal Stimulus(Lang lang1, Lang lang2, int lang1ct, int lang2ct)
        {
            Langs = [];
            Langs.Add(lang1, lang1ct);
            Langs.Add(lang2, lang2ct);
        }

        internal Stimulus(Dictionary<Lang, int> langs)
        {
            Langs = langs;
        }
    }
}
