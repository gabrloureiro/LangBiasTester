using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using LangBiasTester.Operators.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        private List<Audio>? Audios;

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
            Audios = JsonSerializer.Deserialize<List<Audio>>
            (
                File.ReadAllText(Settings.AudioFilePath),
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                }
            );

            _conditionManager.Init(Conditions, Audios, Settings.TestLang1, Settings.TestLang2, Settings.Conditions);
            _trialManager.Init(Trials, Conditions, Audios, Settings.TrialsPerCondition, Settings.Rounds, Settings.ResponseWindow);
        }
    }
}
