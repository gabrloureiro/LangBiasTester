using LangBiasTester.Infrastructure.Managers;
using System.Text.Json;

namespace LangBiasTester.Operators
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            string jsonString = File.ReadAllText("../../../settings.json");

            ExperimentManager? experiment = JsonSerializer.Deserialize<ExperimentManager>(jsonString)
                ?? throw new Exception("Could not create experiment. Check that settings.json exists.");

            Util util = new ();
            
            Form1 form = new (experiment, util);

            Application.Run(form);
        }
    }
}