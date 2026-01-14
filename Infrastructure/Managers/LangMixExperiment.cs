using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class LangMixExperiment : Experiment
    {
        // Experiment properties to be exported
        internal struct MiniSettings
        {
            public List<Lang> Langs { get; set; }
            public List<List<Keys>> LangKeys { get; set; }
            public bool ControlIncluded { get; set; }
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

            InitLangs();

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
                StimulusManager.Init(Stimuli, Audios, Settings.StimulusDuration, Settings.TestLangs, Settings.Stimuli);
            }

            // Initialize trials
            TrialManager.Init(Trials, Stimuli, Settings.TrialBlocks, Settings.SequencesPerTrialBlock, randomizerSeed, Settings.UseT1I1Shuffling);
        }
        private void InitLangs()
        {
            if (Settings.TestLangs.Count != Settings.LangKeys.Count) ErrorManager.ThrowError("Not enough key assignments to languages. Check settings.json.");

            Random random = new(randomizerSeed);

            if (Settings.ShuffleTextLang)
            {
                var langs = Settings.TestLangs.ToArray();
                random.Shuffle(langs);
                Settings.TestLangs = [.. langs];
            }

            if (Settings.ShuffleLangDirection)
            {
                var langKeys = Settings.LangKeys.ToArray();
                random.Shuffle(langKeys);
                Settings.LangKeys = [.. langKeys];
            }

            Settings.LangKeyDictionary = [];
            Settings.ConfidenceRatings = [];
            Settings.ValidKeys = [];

            // Iterate through test languages
            for (int i = 0; i < Settings.TestLangs.Count; i++)
            {
                // For each language, add a {Lang, List<Keys>} pair associating a group of keys with a language
                Settings.LangKeyDictionary.Add(Settings.TestLangs[i], Settings.LangKeys[i]);

                // Compute confidence steps
                var langKeyCount = Settings.LangKeys[i].Count;
                float langConfidenceSteps = 1 / langKeyCount;

                // Iterate through each key in the language's key list
                for (int j = 0; j < langKeyCount; j++)
                {
                    // For each key, associate a confidence rating
                    Settings.ConfidenceRatings.Add(Settings.LangKeys[i][j], (float)Math.Round(langConfidenceSteps * (j + 1), 2));
                }

                // Add keys to list of valid response keys
                Settings.ValidKeys.AddRange(Settings.LangKeys[i]);
            }
        }

        internal override void ExportData()
        {
            ExportSettings = new()
            {
                Langs = Settings.TestLangs,
                ControlIncluded = Settings.IncludeControl,
                LangKeys = Settings.LangKeys,
                StimulusSequence = [..Trials.Select(trial => trial.StimulusNumber)]
            };

            var jsonString = JsonSerializer.Serialize(this, _jsonSerializerOptions);

            File.WriteAllText($"outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
