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
        private readonly DataExporter _dataExporter;
        internal Settings Settings;

        private readonly ConditionManager _conditionManager;
        private readonly TrialManager _trialManager;

        internal List<Condition> Conditions;
        internal LinkedList<Trial> Trials;

        internal ExperimentManager(Settings settings)
        {
            _dataExporter = new();
            Settings = settings;

            _conditionManager = new();
            _trialManager = new();

            Conditions = [];
            Trials = [];
        }

        internal void Init()
        {
            _conditionManager.Init(Conditions);
            _trialManager.InitializeTrials(Trials, Conditions, Settings.TrialsPerCondition, Settings.Rounds, Settings.ResponseWindow);
        }

        internal void RunTrial(Trial trial)
        {
            _trialManager.StartTrial(trial);
        }
    }
}
