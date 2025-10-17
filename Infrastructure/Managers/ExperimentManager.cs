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
        internal struct MiniSettings
        {
            public Lang Lang1 { get; set; }
            public Lang Lang2 { get; set; }
            public bool ControlIncluded { get; set; }
            public Keys Lang1Direction { get; set; }
            public Keys Lang2Direction { get; set; }
        }

        internal Settings Settings;

        [JsonPropertyName("Settings")]
        public MiniSettings ExportSettings { get; set; }

        [JsonInclude]
        internal ParticipantInfo ParticipantInfo { get; set; }

        private readonly ConditionManager _conditionManager;
        private readonly TrialManager _trialManager;

        [JsonInclude]
        public LinkedList<Trial> Trials { get; private set; }
        private readonly List<Condition> Conditions;
        private List<Audio>? Audios;

        internal ExperimentManager(Settings settings, ParticipantInfo participantInfo)
        {
            Settings = settings;

            _conditionManager = new();
            _trialManager = new();

            Conditions = [];
            Trials = [];
            ParticipantInfo = participantInfo;
        }
        internal void Init()
        {
            ShuffleLangs();

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
        private void ShuffleLangs()
        {
            if (Settings.ShuffleTextLang)
            {
                Random random = new();
                var lang1 = Settings.TestLang1;
                var lang2 = Settings.TestLang2;

                Lang[] langs = [lang1, lang2];

                random.Shuffle(langs);

                Settings.TestLang1 = langs[0];
                Settings.TestLang2 = langs[1];
            }
            if (Settings.ShuffleLangDirection)
            {
                Random random = new();
                var dir1 = Settings.Lang1Direction;
                var dir2 = Settings.Lang2Direction;

                Keys[] dirs = [dir1, dir2];

                random.Shuffle(dirs);

                Settings.Lang1Direction = dirs[0];
                Settings.Lang2Direction = dirs[1];
            }
            Settings.LangKeyDictionary = new()
            {
                { Settings.TestLang1, Settings.Lang1Direction },
                { Settings.TestLang2, Settings.Lang2Direction }
            };
        }
        internal void ExportData()
        {
            ExportSettings = new()
            {
                Lang1 = Settings.TestLang1,
                Lang2 = Settings.TestLang2,
                ControlIncluded = Settings.IncludeControl,
                Lang1Direction = Settings.Lang1Direction,
                Lang2Direction = Settings.Lang2Direction
            };

            var jsonString = JsonSerializer.Serialize
                (
                    this,
                    new JsonSerializerOptions()
                    {
                        Converters = { new JsonStringEnumConverter() },
                        WriteIndented = true 
                    }
                );
            File.WriteAllText($"Outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
