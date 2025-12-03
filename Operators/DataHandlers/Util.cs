using LangBiasTester.Infrastructure.Managers;
using LangBiasTester.Infrastructure.Models;
using NAudio.Wave;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LangBiasTester.Operators.DataHandlers
{
    internal class Util
    {
        private readonly JsonSerializerOptions _options = new()
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        /// <summary>
        /// Extracts and deserializes application settings from a JSON file.
        /// </summary>
        /// 
        /// <remarks>This method reads the contents of a JSON file and deserializes it into a <see
        /// cref="Settings"/> object. The file must exist and contain valid JSON that matches the structure of the <see
        /// cref="Settings"/> class.</remarks>
        /// 
        /// <returns>A <see cref="Settings"/> object populated with the deserialized data from the JSON file.</returns>
        /// <exception cref="Exception">Thrown if the deserialization process results in a null <see cref="Settings"/> object.</exception>
        internal Settings ExtractSettings()
        {
            var jsonString = File.ReadAllText("config/Settings.json"); //rewrite so it finds file instead of relying on specific path

            var settings = JsonSerializer.Deserialize<Settings>(jsonString, _options);

            if (settings == null) ErrorManager.ThrowError("Could not configure experiment. Check that settings.json exists.");

            return settings;
        }

        /// <summary>
        /// Extracts metadata from audio files located in the specified directory and generates a JSON file containing
        /// the metadata.
        /// </summary>
        /// 
        /// <remarks>This method scans the directory specified in <paramref name="settings"/> for audio
        /// files organized by language and voice gender. It extracts metadata such as file name, file location,
        /// duration, language, and voice gender for each audio file with a `.wav` extension. The extracted metadata is
        /// serialized into a JSON file named "AudioMetadata.json" and saved in the parent directory of the audio files.
        /// If "AudioMetadata.json" already exists in the directory and <paramref name="overrideExisting"> is set to false,
        /// the method exits without performing any operations.</remarks>
        /// 
        /// <param name="settings">The settings object containing the path to the directory where audio files are stored.</param>
        /// <exception cref="Exception">Thrown if the directory structure does not follow the expected folder naming conventions for language or
        /// voice gender.</exception>
        internal void ExtractRawAudioMetadata(Settings settings, bool overrideExisting)
        {
            var audioDirectory = (new DirectoryInfo(settings.RawAudioFilePath)).Parent;

            if (File.Exists(settings.RawAudioFilePath) && !overrideExisting) return;

            if (audioDirectory.GetDirectories().Length == 0) ErrorManager.ShowError("Audio directories not found.");

            List<Audio> audios = [];

            foreach (DirectoryInfo language in audioDirectory.GetDirectories())
            {
                if (language.Name == "Mixed") continue;
                if (!Enum.TryParse(language.Name, out Lang lang)) ErrorManager.ShowError($"Incorrect naming format: {language.Name}.");
                if (language.GetDirectories().Length == 0) ErrorManager.ShowError("Voice gender directories not found.");

                foreach (DirectoryInfo voiceGender in language.GetDirectories())
                {
                    if (!Enum.TryParse(voiceGender.Name, out VoiceGender gender)) ErrorManager.ShowError($"Incorrect naming format: {voiceGender.Name}.");

                    foreach (FileInfo audioFile in voiceGender.GetFiles())
                    {
                        if (!(audioFile.Extension == ".wav")) continue;

                        var metadata = new Audio
                        {
                            FileName = audioFile.Name,
                            FileLocation = audioFile.FullName,

                            Duration = new AudioFileReader(audioFile.FullName).TotalTime,
                            Lang = lang,
                            VoiceGender = gender
                        };
                        audios.Add(metadata);
                    }
                }
            }
            var jsonString = JsonSerializer.Serialize(audios, _options);
            File.WriteAllText($"{audioDirectory}/RawAudioMetadata.json", jsonString);
        }

        /// <summary>
        /// Creates static audio mixes based on the provided settings and audio data.
        /// </summary>
        /// 
        /// <remarks>This method reads audio data from a JSON file specified in the <paramref name="settings"/> parameter.
        /// It filters the audio data by gender and language, then creates static audio mixes of different proportions of each language
        /// by combining an equal number of female and male voices from each language. Finally, it generates a JSON file with the
        /// specifications of the mixed audios.</remarks>
        /// 
        /// <param name="settings">The configuration settings that specify the audio file path and test languages.</param>
        /// <exception cref="Exception">Thrown if the audio data cannot be deserialized from the specified file or if the file does not exist.</exception>
        internal void CreateStaticAudioMixes(Settings settings)
        {
            List<Audio> mixedAudios = [];
            var mixedAudioDirectory = (new DirectoryInfo(settings.MixedAudioFilePath)).Parent;

            if (!File.Exists(settings.RawAudioFilePath)) ExtractRawAudioMetadata(settings, true);

            var audioJsonString = File.ReadAllText(settings.RawAudioFilePath);

            var audioJson = JsonSerializer.Deserialize<IEnumerable<Audio>>(audioJsonString, _options).ToList();
            if (audioJson == null) ErrorManager.ThrowError("Could not configure experiment. Check that AudioMetadata.json exists.");

            List<Audio> femaleAudios = [..audioJson.Where(audio =>
                audio.VoiceGender == VoiceGender.Female
                && (audio.Lang == settings.TestLang1 || audio.Lang == settings.TestLang2)
                )];

            List<Audio> maleAudios = [.. audioJson.Where(audio =>
                audio.VoiceGender == VoiceGender.Male
                && (audio.Lang == settings.TestLang1 || audio.Lang == settings.TestLang2)
                )];

            if (femaleAudios.Count != maleAudios.Count) ErrorManager.ThrowError($"Voice gender distribution is not equal ({femaleAudios.Count} female audios and {maleAudios.Count} male audios).");

            var totalAudios = femaleAudios.Count + maleAudios.Count;

            var lang2FemaleCount = 0;
            var lang2MaleCount = 0;

            var audioFolderString = Directory.GetParent(settings.RawAudioFilePath).ToString() + "/Mixed";
            Directory.CreateDirectory(audioFolderString);

            var progressText = CreateProgressForm();

            for (int i = 0; i <= totalAudios/2; i++)
            {
                List<Audio> audiosToMix = [];

                audiosToMix.AddRange(femaleAudios.Where(audio => audio.Lang == settings.TestLang1).Take(femaleAudios.Count/2 - lang2FemaleCount));
                audiosToMix.AddRange(maleAudios.Where(audio => audio.Lang == settings.TestLang1).Take(maleAudios.Count / 2 - lang2MaleCount));

                audiosToMix.AddRange(femaleAudios.Where(audio => audio.Lang == settings.TestLang2).Take(lang2FemaleCount));
                audiosToMix.AddRange(maleAudios.Where(audio => audio.Lang == settings.TestLang2).Take(lang2MaleCount));

                if (i % 2 == 0) lang2FemaleCount++;
                else lang2MaleCount++;

                mixedAudios.Add(AudioManager.CreateStaticAudioMix(audiosToMix, audioFolderString, progressText));
            }

            progressText.AppendText(Environment.NewLine);
            progressText.AppendText("Static audio mixes created.");

            var jsonString = JsonSerializer.Serialize(mixedAudios, _options);
            File.WriteAllText($"{mixedAudioDirectory}/MixedAudioMetadata.json", jsonString);

            progressText.AppendText(Environment.NewLine);
            progressText.AppendText("MixedAudioMetadata.json written.");
        }
        private TextBox CreateProgressForm()
        {
            Form progressForm = new()
            {
                Text = "Creating Static Audio Mixes",
                Size = new Size(600, 400)
            };

            TextBox progressText = new()
            {
                Text = $"Initializing mix creation.",
                Multiline = true,
                Size = new Size(580, 320),
                ScrollBars = ScrollBars.Vertical
            };

            Button doneButton = new()
            {
                Text = "Done",
                Size = new Size(75, 30),
                Location = new Point((progressForm.ClientSize.Width - 75) / 2, progressForm.ClientSize.Height - 40),
                Anchor = AnchorStyles.Bottom
            };

            doneButton.Click += (s, e) =>
            {
                progressForm.Close();
                progressForm.Dispose();
            };

            progressForm.Controls.Add(progressText);
            progressForm.Controls.Add(doneButton);
            progressForm.Show();

            return progressText;
        }
    }
}
