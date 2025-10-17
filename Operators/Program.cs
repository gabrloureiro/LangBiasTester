using LangBiasTester.Infrastructure.Managers;
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
            util.ExtractAudioMetadata();
            ExperimentManager experiment = new(util.ExtractSettings());

            Form1 form = new (experiment, util);

            Application.Run(form);
        }
    }
}