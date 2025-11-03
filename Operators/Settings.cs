using LangBiasTester.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static LangBiasTester.Infrastructure.Models.Lang;

namespace LangBiasTester.Operators
{
    internal class Settings
    {
        public int AdminID { get; set; }
        public bool RunDemo { get; set; }


        public bool RunTraining { get; set; }
        public int TrainingTrials { get; set; }
        public bool TestReactionTime { get; set; }


        public int TimeBetweenTrials { get; set; }
        public int ResponseWindow { get; set; }
        public int TrialsPerCondition { get; set; }
        public int Rounds { get; set; }


        public Lang TestLang1 { get; set; } //include
        public Lang TestLang2 { get; set; } //include
        public bool IncludeControl { get; set; } //include
        public Lang ControlLang { get; set; }

        
        public Keys Lang1Direction { get; set; } //include
        public Keys Lang2Direction { get; set; } //include

        public bool ShuffleTextLang { get; set; }
        public bool ShuffleLangDirection { get; set; }

        public List<List<int>>? Conditions { get; set; }
        public string? AudioFilePath { get; set; }

        [JsonIgnore]
        internal Dictionary<Lang, Keys>? LangKeyDictionary { get; set; }
    }
}
