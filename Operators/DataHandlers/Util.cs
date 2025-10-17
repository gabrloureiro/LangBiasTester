using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LangBiasTester.Operators.DataHandlers
{
    internal class Util
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        internal Settings ExtractSettings()
        {
            var jsonString = File.ReadAllText("../../../settings.json"); //rewrite so it finds file instead of relying on specific path

            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, _options)
                ?? throw new Exception("Could not configure experiment. Check that settings.json exists.");

            return settings;
        }
        internal void ExtractAudioMetadata()
        {
            var audioDirectory = new DirectoryInfo("../../../Audios");

            if (audioDirectory.GetFiles("AudioMetadata.json").Length > 0) return;

            List<Audio> audios = [];

            foreach (DirectoryInfo directory in audioDirectory.GetDirectories())
            {
                if (Enum.TryParse(directory.Name, out Lang lang))
                {
                    int i = 0;
                    foreach (FileInfo audioFile in directory.GetFiles())
                    {
                        if (audioFile.Extension == ".wav")
                        {
                            var metadata = new Audio
                            {
                                FileName = audioFile.Name,
                                FileLocation = audioFile.FullName,

                                Duration = audioFile.Length, //fix so it's actually the duration, not the file size
                                Lang = lang
                            };

                            audios.Add(metadata);
                            i++;
                        }
                    }
                }
                else throw new Exception($"Incorrect naming format: {directory.Name}.");
            }
            var jsonString = JsonSerializer.Serialize(audios, _options);
            File.WriteAllText($"{audioDirectory}/AudioMetadata.json", jsonString);
        }
    }
}
