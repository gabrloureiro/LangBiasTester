using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators.DataHandlers;
using System.Text.Json;

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
            util.ExtractAudioMetadata(settings, false);

            ExperimentManager experiment = new(settings, new ParticipantInfo());

            Form1 form = new (experiment, util);

            Application.Run(form);
        }
    }
}