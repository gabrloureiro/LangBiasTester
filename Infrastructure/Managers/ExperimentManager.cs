using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class ExperimentManager
    {
        internal Experiment Experiment;
        internal ConditionManager ConditionManager;
        internal int AudioDuration;
        internal int TimeBetweenTrials;
        internal int ResponseWindow;

        internal ExperimentManager(int audioDuration, int timeBetweenTrials, int responseWindow)
        {
            Experiment = new Experiment();
            ConditionManager = new ConditionManager();

            AudioDuration = audioDuration;
            TimeBetweenTrials = timeBetweenTrials;
            ResponseWindow = responseWindow;
        }

        internal void Initialize()
        {
            Experiment.Rounds.Initialize(ConditionManager.Conditions, AudioDuration);
        }

        internal void Run()
        {
            Experiment.Rounds.Run(TimeBetweenTrials, ResponseWindow);
        }
    }
}
