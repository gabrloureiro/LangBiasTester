using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class LangMixExperiment : Experiment
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

        [JsonPropertyName("Settings")]
        internal MiniSettings ExportSettings { get; set; }

        private readonly List<LangAudio> Audios;
        private int randomizerSeed;

        internal LangMixExperiment(Settings settings, ParticipantInfo participantInfo) : base(settings, participantInfo)
        {
            Audios = [];
        }
        
        internal override void Init()
        {
            randomizerSeed = ParticipantInfo.ID;

            ShuffleLangs();

            var audioFilePath = Settings.UseStaticMixes ? Settings.MixedAudioFilePath : Settings.RawAudioFilePath;

            Audios.AddRange(JsonSerializer.Deserialize<List<LangAudio>>(File.ReadAllText(audioFilePath),_jsonSerializerOptions));

            // Assign audios to stimuli.
            // If using static mixes, all information is already contained in the audio objects.
            // Otherwise, generate stimuli one by one using information in settings.
            if (Settings.UseStaticMixes)
            {
                StimulusManager.Init(Stimuli, Audios, Settings.StimulusDuration);
            }
            else
            {
                StimulusManager.Init(Stimuli, Audios, Settings.StimulusDuration, Settings.TestLang1, Settings.TestLang2, Settings.Stimuli);
            }

            // Initialize trials
            TrialManager.Init(Trials, Stimuli, Settings.TrialBlocks, Settings.SequencesPerTrialBlock, randomizerSeed, Settings.UseT1I1Shuffling);
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

        internal override void ExportData()
        {
            ExportSettings = new ()
            {
                Lang1 = Settings.TestLang1,
                Lang2 = Settings.TestLang2,
                ControlIncluded = Settings.IncludeControl,
                Lang1Direction = Settings.Lang1Direction,
                Lang2Direction = Settings.Lang2Direction,
                StimulusSequence = [..Trials.Select(trial => trial.StimulusNumber)]
            };

            var jsonString = JsonSerializer.Serialize(this, _jsonSerializerOptions);

            File.WriteAllText($"outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
