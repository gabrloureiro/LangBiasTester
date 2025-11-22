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
        // Settings properties
        internal struct MiniSettings
        {
            public Lang Lang1 { get; set; }
            public Lang Lang2 { get; set; }
            public bool ControlIncluded { get; set; }
            public Keys Lang1Direction { get; set; }
            public Keys Lang2Direction { get; set; }
            public List<int> ConditionSequence { get; set; }
        }

        internal Settings Settings;

        [JsonPropertyName("Settings")]
        public MiniSettings ExportSettings { get; set; }

        // Participant Info
        [JsonInclude]
        internal ParticipantInfo ParticipantInfo { get; set; }

        // Lists
        [JsonInclude]
        public LinkedList<Trial> Trials { get; private set; }
        private readonly List<Condition> Conditions;
        private List<Audio>? Audios;

        private JsonSerializerOptions _jsonSerializerOptions;

        internal ExperimentManager(Settings settings, ParticipantInfo participantInfo)
        {
            Settings = settings;

            Conditions = [];
            Trials = [];
            ParticipantInfo = participantInfo;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        }
        
        internal void Init()
        {
            ShuffleLangs();

            Audios = JsonSerializer.Deserialize<List<Audio>>(File.ReadAllText(Settings.RawAudioFilePath),_jsonSerializerOptions);

            ConditionManager.Init(Conditions, Audios, Settings.TestLang1, Settings.TestLang2, Settings.Conditions);
            TrialManager.Init(Trials, Conditions, Audios, Settings.ResponseWindow, ParticipantInfo.ID);

            //set ConditionSequence for export based on what TrialManager.Init generates
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

            var jsonString = JsonSerializer.Serialize(this, _jsonSerializerOptions);

            File.WriteAllText($"Outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
