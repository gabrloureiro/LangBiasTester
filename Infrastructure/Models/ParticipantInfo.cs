using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Models
{
    internal class ParticipantInfo
    {
        [JsonInclude]
        internal int ID { get; set; }
        internal Lang PreferredLang { get; set; } // should be modified once preferred language criteria is established (as per Dr. Titone's suggestions)
    }
}
