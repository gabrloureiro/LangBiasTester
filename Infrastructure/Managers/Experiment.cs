using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class Experiment
    {
        // Experiment properties to be exported
        internal struct MiniSettings
        {
            public Lang Lang1 { get; set; }
            public Lang Lang2 { get; set; }
            public bool ControlIncluded { get; set; }
            public Keys Lang1Direction { get; set; }
            public Keys Lang2Direction { get; set; }
            public int[] StimulusSequence { get; set; }
        }

        internal Settings Settings;

        [JsonPropertyName("Settings")]
        internal MiniSettings ExportSettings { get; set; }

        // Participant Info
        [JsonInclude]
        internal ParticipantInfo ParticipantInfo { get; set; }

        // Lists
        [JsonInclude]
        internal LinkedList<Trial> Trials { get; private set; }
        private readonly List<Stimulus> Stimuli;
        private readonly List<Audio> Audios;

        private JsonSerializerOptions _jsonSerializerOptions;
        private int randomizerSeed;

        internal Experiment(Settings settings, ParticipantInfo participantInfo)
        {
            Settings = settings;

            Stimuli = [];
            Trials = [];
            Audios = [];
            ParticipantInfo = participantInfo;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        }
        
        internal void Init()
        {
            randomizerSeed = ParticipantInfo.ID;

            ShuffleLangs();

            var audioFilePath = Settings.UseStaticMixes ? Settings.MixedAudioFilePath : Settings.RawAudioFilePath;

            Audios.AddRange(JsonSerializer.Deserialize<List<Audio>>(File.ReadAllText(audioFilePath),_jsonSerializerOptions));

            // If using static mixes, all information is already contained in the audio objects.
            // Otherwise, generate stimuli one by one using information in settings.
            if (Settings.UseStaticMixes)
            {
                StimulusManager.Init(Stimuli, Audios);
            }
            else
            {
                StimulusManager.Init(Stimuli, Audios, Settings.TestLang1, Settings.TestLang2, Settings.Stimuli);
            }

            // Initialize trials
            TrialManager.Init(Trials, Stimuli, Audios, Settings.ResponseWindow, Settings.TrialBlocks, Settings.SequencesPerTrialBlock, randomizerSeed, Settings.UseT1I1Shuffling);
        }
        private void ShuffleLangs()
        {
            if (Settings.ShuffleTextLang)
            {
                Random random = new(randomizerSeed);
                var lang1 = Settings.TestLang1;
                var lang2 = Settings.TestLang2;

                Lang[] langs = [lang1, lang2];

                random.Shuffle(langs);

                Settings.TestLang1 = langs[0];
                Settings.TestLang2 = langs[1];
            }
            if (Settings.ShuffleLangDirection)
            {
                Random random = new(randomizerSeed);
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
                Lang2Direction = Settings.Lang2Direction,
                StimulusSequence = [.. Trials.Select(trial => trial.StimulusNumber)]
            };

            var jsonString = JsonSerializer.Serialize(this, _jsonSerializerOptions);

            File.WriteAllText($"outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
