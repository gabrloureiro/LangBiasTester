using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ExperimentManager
    {
        internal ConditionManager ConditionManager;
        internal TrialManager TrialManager;
        internal List<Condition> Conditions;
        internal LinkedList<Trial> Trials;
        internal DataExporter DataExporter;

        internal int TimeBetweenTrials;
        internal int ResponseWindow;
        internal int Rounds;

        public ExperimentManager()
        {
            ConditionManager = new ConditionManager();
            TrialManager = new TrialManager();
            Conditions = [];
            Trials = [];

            DataExporter = new DataExporter();
        }

        internal void Initialize()
        {
            //Experiment.Rounds.Initialize(ConditionManager.Conditions, AudioDuration);
        }

        internal void Run()
        {
            //Experiment.Rounds.Run(TimeBetweenTrials, ResponseWindow);
        }
    }
}
