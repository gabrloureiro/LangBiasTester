using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Managers;

namespace LangBiasTester.Infrastructure.Models
{
    internal class Trial
    {
        [JsonInclude]
        internal Condition Condition { get; set; }
        [JsonInclude]
        internal List<Audio>? Audios { get; set; }
        [JsonInclude]
        internal Audio? MixedAudio { get; set; }
        [JsonInclude]
        internal Keys? Response { get; set; }
        [JsonInclude]
        internal Lang? ResponseLang { get; set; }
        [JsonInclude]
        internal int? Round { get; set; }
        
        internal Trial(Condition condition)
        {
            Condition = condition;
        }
    }
}
