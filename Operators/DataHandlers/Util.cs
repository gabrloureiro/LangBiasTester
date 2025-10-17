using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using NAudio.Wave;
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
            var jsonString = File.ReadAllText("Settings.json"); //rewrite so it finds file instead of relying on specific path

            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, _options)
                ?? throw new Exception("Could not configure experiment. Check that settings.json exists.");

            return settings;
        }
        internal void ExtractAudioMetadata(Settings settings)
        {
            var audioDirectory = (new DirectoryInfo(settings.AudioFilePath)).Parent;

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

                                Duration = new AudioFileReader(audioFile.FullName).TotalTime,
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
        //TODO: generate a large number of sequences using the matlab script and save them to a JSON file.
        //Each time the program runs, a new sequence is selected to generate the trials
        //Use logic similar to ExtractAudioMetadata to run automatically when the program starts if the file is not found
        internal void CollectT1I1Sequences()
        {
            //MLApp.MLApp matlab = new MLApp.MLApp();
        }
    }
}
