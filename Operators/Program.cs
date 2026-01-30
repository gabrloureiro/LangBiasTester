using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;

namespace LangBiasTester.Operators
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Util util = new();
            LangSettings settings = util.ExtractSettings();
            util.ExtractRawAudioMetadata(settings, false);

            LangMixExperiment experiment = new(settings, new ParticipantInfo(), util);

            ExperimentView form = new (experiment);

            Application.Run(form);
        }
    }
}