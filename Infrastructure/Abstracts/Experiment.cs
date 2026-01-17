using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Infrastructure.Abstracts
{
    internal abstract class Experiment
    {
        internal Settings Settings;
        internal ParticipantInfo ParticipantInfo;
        internal Instructions Instructions;

        [JsonInclude]
        internal LinkedList<Trial> Trials { get; private set; }
        internal readonly List<Stimulus> Stimuli;

        internal readonly JsonSerializerOptions _jsonSerializerOptions;

        internal Experiment(Settings settings, ParticipantInfo participantInfo)
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

        internal abstract void Init();
        internal abstract void ExportData();
    }
}
