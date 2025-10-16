using LangBiasTester.Infrastructure.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LangBiasTester.Operators.DataHandlers
{
    internal class Util
    {
        //TODO (include default setting)
        internal Settings ExtractSettings()
        {
            var jsonString = File.ReadAllText("../../../settings.json"); //rewrite so it finds file instead of relying on specific path

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, options)
                ?? throw new Exception("Could not create experiment. Check that settings.json exists.");

            return settings;
        }
        internal void ExtractAudioMetadata()
        {

        }
    }
}
