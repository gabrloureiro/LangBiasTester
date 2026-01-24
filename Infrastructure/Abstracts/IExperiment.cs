using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using LangBiasTester.View;

namespace LangBiasTester.Infrastructure.Abstracts
{
    internal interface IExperiment
    {
        ParticipantInfo ParticipantInfo { get; }
        LinkedList<Trial> Trials { get; }
        List<Stimulus> Stimuli { get; }
        Settings Settings { get; }
        SettingsForm SettingsUI { get; set; }
        Instructions Instructions { get; }

        void Init();
        void ExportData();
        void SetTrialResponse(Trial trial);
    }
}
