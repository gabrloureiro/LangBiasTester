using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Operators.DataHandlers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class LangMixExperiment : Experiment<LangInstructions, LangSettings>
    {
        // Experiment properties
        private readonly List<LangAudio> Audios;
        private int _randomizerSeed;
        internal readonly Util _util;

        // Experiment settings to be exported
        [JsonInclude, JsonPropertyName("ExperimentSettings")]
        internal ExportSettings? ExportSettings;

        // Constructor
        internal LangMixExperiment(LangSettings settings, ParticipantInfo participantInfo, Util util) : base(settings, participantInfo)
        {
            _util = util;
            SettingsUI = new(settings, util);
            Audios = [];
            Instructions = new LangInstructions(Settings);
        }

        /// <summary>
        /// Initialize experiment
        /// </summary>
        public override void Init()
        {
            _randomizerSeed = ParticipantInfo.ID;

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
            else if (Settings.UseExplicitMorphSteps)
            {
                StimulusManager.Init(Stimuli, Audios, Settings.StimulusDuration, Settings.TestLangs, Settings.MorphSteps);
            }
            else
            {
                // initialize stimuli using morph settings
            }

            // Initialize trials
            TrialManager.Init(Trials, Stimuli, Settings.TrialBlocks, Settings.SequencesPerTrialBlock, _randomizerSeed);

            Instructions.Init();
        }
        /// <summary>
        /// Initialize language settings
        /// </summary>
        private void InitLangs()
        {
            if (Settings.TestLangs.Count != Settings.LangKeys.Count) ErrorManager.ThrowError("Not enough key assignments to languages. Check settings.json.");

            Random random = new(_randomizerSeed);

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
                float langConfidenceSteps = 1 / (float)langKeyCount;

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

        public override void SetTrialResponse(Trial trial)
        {
            if (trial.Response != null)
            {
                var response = (Keys)trial.Response;
                trial.ResponseLang = Settings.LangKeyDictionary.Where(entry => entry.Value.Contains(response)).First().Key;
            }
        }

        public override void ExportData()
        {
            ExportSettings = new()
            {
                Langs = Settings.TestLangs,
                ControlIncluded = Settings.IncludeControl,
                LangKeys = Settings.LangKeyDictionary,
                StimulusSequence = [..Trials.Select(trial => trial.StimulusNumber)]
            };

            var jsonString = JsonSerializer.Serialize(this, _jsonSerializerOptions);

            File.WriteAllText($"outputs/{ParticipantInfo.ID}.json", jsonString);
        }
    }
}
