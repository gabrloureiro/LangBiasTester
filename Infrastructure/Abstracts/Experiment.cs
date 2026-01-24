using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using LangBiasTester.View;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Abstracts
{
    internal abstract class Experiment<TInstructions, TSettings> : IExperiment
        where TInstructions : Instructions
        where TSettings : Settings
    {
        [JsonIgnore]
        public TInstructions Instructions { get; set; }
        [JsonIgnore]
        public TSettings Settings { get; set; }

        [JsonIgnore]
        public SettingsForm SettingsUI { get; set; }

        public ParticipantInfo ParticipantInfo { get; set; }

        [JsonInclude]
        public LinkedList<Trial> Trials { get; private set; }
        public List<Stimulus> Stimuli { get; private set; }

        [JsonIgnore]
        internal readonly JsonSerializerOptions _jsonSerializerOptions;

        [JsonIgnore]
        Instructions IExperiment.Instructions => Instructions;
        [JsonIgnore]
        Settings IExperiment.Settings => Settings;

        protected Experiment(TSettings settings, ParticipantInfo participantInfo)
        {
            Settings = settings;

            Stimuli = [];
            Trials = [];

            ParticipantInfo = participantInfo;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        }

        public abstract void Init();
        public abstract void ExportData();
        public abstract void SetTrialResponse(Trial trial);
    }
}
