using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using LangBiasTester.Operators.DataHandlers;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ExperimentManager
    {
        private readonly DataExporter _dataExporter = new();
        internal Settings _settings = new(42);

        private readonly ConditionManager _conditionManager = new();
        private readonly TrialManager _trialManager = new();

        internal List<Condition> Conditions = [];
        internal LinkedList<Trial> Trials = [];

        internal int TimeBetweenTrials { get; set; }
        internal int ResponseWindow { get; set; }
        internal int Rounds { get; set; }
        internal int TrialsPerCondition { get; set; }

        internal void Initialize()
        {
            _trialManager.InitializeTrials(Trials, Conditions, TrialsPerCondition, Rounds, ResponseWindow);
        }

        internal void RunTrial(Trial trial)
        {
            _trialManager.StartTrial(trial);
        }
    }
}
