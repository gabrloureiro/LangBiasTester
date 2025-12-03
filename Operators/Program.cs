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
            Settings settings = util.ExtractSettings();
            util.ExtractRawAudioMetadata(settings, false);

            Experiment experiment = new(settings, new ParticipantInfo());

            ProgramView form = new (experiment, util);

            Application.Run(form);
        }
    }
}