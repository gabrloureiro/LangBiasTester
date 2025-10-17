using LangBiasTester.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LangBiasTester.Infrastructure.Models.Lang;

namespace LangBiasTester.Operators
{
    internal class Settings
    {
        public int AdminID { get; set; }


        public bool RunTraining { get; set; }
        public int TrainingTrials { get; set; }


        public int TimeBetweenTrials { get; set; }
        public int ResponseWindow { get; set; }
        public int TrialsPerCondition { get; set; }
        public int Rounds { get; set; }


        public Lang TestLang1 { get; set; }
        public Lang TestLang2 { get; set; }
        public bool IncludeControl { get; set; }
        public Lang ControlLang { get; set; }

        public List<List<int>>? Conditions { get; set; }
        public string? AudioFilePath { get; set; }
    }
}
