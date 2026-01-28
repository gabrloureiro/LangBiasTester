using LangBiasTester.Infrastructure.Enums;
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
        private static readonly Random _rnd = new();

        /// <summary>
        /// Extracts and deserializes application settings from a JSON file.
        /// </summary>
        /// <remarks>
        /// This method reads the contents of a JSON file and deserializes it into a <see cref="LangSettings"/> object.
        /// The file must exist and contain valid JSON that matches the structure of the <see cref="LangSettings"/> class.
        /// </remarks>
        /// <returns>A <see cref="LangSettings"/> object populated with the deserialized data from the JSON file.</returns>
        /// <exception cref="Exception">Thrown if the deserialization process results in a null <see cref="LangSettings"/> object.</exception>
        internal LangSettings ExtractSettings() // TODO: CHANGE SO THAT CAN DESERIALIZE TO ANY TYPE OF SETTINGS
        {
            var jsonString = File.ReadAllText("config/Settings.json");

            var settings = JsonSerializer.Deserialize<LangSettings>(jsonString, _options);

            if (settings == null) ErrorManager.ThrowError("Could not configure experiment. Check that settings.json exists.");

            if (settings.TrialBlocks < 1) ErrorManager.ThrowError($"At least one trial block is needed (TrialBlocks currently set to {settings.TrialBlocks}).");

            return settings;
        }

        /// <summary>
        /// Extracts metadata from audio files located in the specified directory and generates a JSON file containing
        /// the metadata.
        /// </summary>
        /// <remarks>
        /// This method scans the directory specified in <paramref name="settings"/> for audio
        /// files organized by language and voice gender. It extracts metadata such as file name, file location,
        /// duration, language, and voice gender for each audio file with a `.wav` extension. The extracted metadata is
        /// serialized into a JSON file named "AudioMetadata.json" and saved in the parent directory of the audio files.
        /// If "AudioMetadata.json" already exists in the directory and <paramref name="overrideExisting"> is set to false,
        /// the method exits without performing any operations.
        /// </remarks>
        /// <param name="settings">The settings object containing the path to the directory where audio files are stored.</param>
        /// <exception cref="Exception">Thrown if the directory structure does not follow the expected folder naming conventions for language or
        /// voice gender.</exception>
        internal void ExtractRawAudioMetadata(LangSettings settings, bool overrideExisting)
        {
            var audioDirectory = (new DirectoryInfo(settings.RawAudioFilePath)).Parent;

            // Exit method if AudioMetadata.json already exists and overrideExisting is false
            if (File.Exists(settings.RawAudioFilePath) && !overrideExisting) return;

            // Check that audio directory structure is correct
            if (audioDirectory.GetDirectories().Length == 0)
            {
                ErrorManager.ShowError("Audio directories not found.");
                return;
            }

            List<LangAudio> audios = [];

            // Iterate through language directories
            foreach (DirectoryInfo language in audioDirectory.GetDirectories())
            {
                // If directory is "Mixed", skip it
                if (language.Name == "Mixed") continue;
                // Check that language directory name is correct
                if (!Enum.TryParse(language.Name, out Lang lang))
                {
                    ErrorManager.ShowError($"Incorrect naming format: {language.Name}.");
                    return;
                }
                // Check that voice gender directories exist
                if (language.GetDirectories().Length == 0)
                {
                    ErrorManager.ShowError("Voice gender directories not found.");
                    return;
                }

                foreach (DirectoryInfo voiceGender in language.GetDirectories())
                {
                    // Check that voice gender directory name is correct
                    if (!Enum.TryParse(voiceGender.Name, out VoiceGender gender))
                    {
                        ErrorManager.ShowError($"Incorrect naming format: {voiceGender.Name}.");
                        return;
                    };

                    // Iterate through audio files for each gender
                    foreach (FileInfo audioFile in voiceGender.GetFiles())
                    {
                        // Only process .wav files
                        if (audioFile.Extension != ".wav") continue;

                        // Extract metadata
                        var metadata = new LangAudio
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

            // Write AudioMetadata JSON file to register information about raw audios
            var jsonString = JsonSerializer.Serialize(audios, _options);
            File.WriteAllText($"{audioDirectory}/RawAudioMetadata.json", jsonString);
        }
                
        private List<LangAudio> GetRawAudioList(LangSettings settings)
        {
            // Read raw audio metadata from JSON file
            var audioJsonString = File.ReadAllText(settings.RawAudioFilePath);
            var allAudios = JsonSerializer.Deserialize<IEnumerable<LangAudio>>(audioJsonString, _options).ToList();
            if (allAudios == null) ErrorManager.ThrowError("Could not configure experiment. Check that AudioMetadata.json exists.");

            // Trim audio list to desired count
            if (settings.AudioCount > allAudios.Count)
            {
                ErrorManager.ShowError($"Requested audio count ({settings.AudioCount}) exceeds available audios ({allAudios.Count}). Using all available audios.");
                settings.AudioCount = allAudios.Count;
            }

            return allAudios;
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
        internal void CreateStaticAudioMixes(LangSettings settings)
        {
            var rawAudios = GetRawAudioList(settings);

            // Find AudioMetadata JSON file, create if it does not exist
            var mixedAudioDirectory = (new DirectoryInfo(settings.MixedAudioFilePath)).Parent;
            if (!File.Exists(settings.RawAudioFilePath)) ExtractRawAudioMetadata(settings, true);

            // Check that audios are long enough for requested snippet duration and sequences per block
            var sequencesPerBlock = settings.SequencesPerTrialBlock;
            var minAudioDuration = rawAudios.Select(audio => audio.Duration).Min().TotalSeconds;
            var snippetDuration = (double)settings.StimulusDuration;

            if (minAudioDuration / sequencesPerBlock < snippetDuration)
            {
                ErrorManager.ThrowError($"Cannot create {sequencesPerBlock} snippets of {snippetDuration} seconds from shortest audio (duration {minAudioDuration}).");
            }

            // Generate audios for each block, sequence, and round
            IterativelyWriteAudios(settings, rawAudios, mixedAudioDirectory);
        }

        /// <summary>
        /// Separates female and male audios
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="allAudios"></param>
        /// <returns></returns>
        private static Dictionary<VoiceGender, List<LangAudio>> SeparateMaleAndFemaleAudios(LangSettings settings, List<LangAudio> allAudios)
        {
            List<LangAudio> femaleAudios = [.. ExtractVoicesByGender(allAudios, VoiceGender.Female, settings.TestLangs).Take(settings.AudioCount * settings.TestLangs.Count)];
            List<LangAudio> maleAudios = [.. ExtractVoicesByGender(allAudios, VoiceGender.Male, settings.TestLangs).Take(settings.AudioCount * settings.TestLangs.Count)];

            // Show error and exit method if voice gender distribution is not equal
            if (femaleAudios.Count != maleAudios.Count)
            {
                ErrorManager.ThrowError($"Voice gender distribution is not equal ({femaleAudios.Count} female audios and {maleAudios.Count} male audios).");
            }

            if (femaleAudios.Count > settings.AudioCount || maleAudios.Count > settings.AudioCount)
            {
                ErrorManager.ThrowError($"Number of audios requested exceeds number of audios for one or both genders.\nAudios requested: {settings.AudioCount} per language, {settings.AudioCount * settings.TestLangs.Count} total.\nTotal male audios: {maleAudios.Count}, total female audios: {femaleAudios.Count})");
            }
            return new()
            {
                { VoiceGender.Female, femaleAudios },
                { VoiceGender.Male, maleAudios }
            };
        }
        
        /// <summary>
        /// Iterates through blocks, sequences, and rounds, and create audios for each
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="audios"></param>
        /// <param name="mixedAudioDirectory"></param>
        private void IterativelyWriteAudios(LangSettings settings, List<LangAudio> audios, DirectoryInfo mixedAudioDirectory)
        {
            // Convert snippet duration to TimeSpan
            var snippetDuration = TimeSpan.FromSeconds(settings.StimulusDuration);

            // Split audios by voice gender
            var splitAudios = SeparateMaleAndFemaleAudios(settings, audios);
            var femaleAudios = splitAudios[VoiceGender.Female];
            var maleAudios = splitAudios[VoiceGender.Male];

            // Get block, sequence, and round counts
            var blockCount = (float)settings.TrialBlocks;
            var sequencesPerBlock = settings.SequencesPerTrialBlock;
            var roundsPerSequence = settings.UseExplicitMorphSteps ? settings.MorphSteps.Count : settings.MorphCount;

            // Get morph steps to only mix audios with the given proportions
            var morphSteps = GetMorphSteps(settings);

            // Create directory for mixed audios
            var audioFolderString = Directory.GetParent(settings.RawAudioFilePath).ToString() + "/Mixed";
            Directory.CreateDirectory(audioFolderString);

            // Create form to show progress
            var progressText = CreateProgressForm();

            var audioCoords = new List<KeyValuePair<List<int>, List<LangAudio>>>();

            for (int blockNumber = 0; blockNumber < blockCount; blockNumber++)
            {
                var blockFolder = audioFolderString + $"/Block_{blockNumber}";

                Directory.CreateDirectory(blockFolder);

                for (int sequenceNumber = 0; sequenceNumber < sequencesPerBlock; sequenceNumber++)
                {
                    var startingTimestamp = sequenceNumber * snippetDuration;

                    var sequenceFolder = blockFolder + $"/Seq_{blockNumber}_{sequenceNumber}";

                    Directory.CreateDirectory(sequenceFolder);

                    for (int roundNumber = 0; roundNumber < roundsPerSequence; roundNumber++)
                    {
                        var roundFolder = sequenceFolder + $"/Round_{blockNumber}_{sequenceNumber}_{roundNumber}";

                        Directory.CreateDirectory(roundFolder);

                        var nestingInfo = new List<int>([blockNumber, sequenceNumber, roundNumber]);

                        var roundAudios = WriteAudios
                            (
                            settings, morphSteps,
                            femaleAudios, maleAudios,
                            mixedAudioDirectory, roundFolder, nestingInfo,
                            startingTimestamp, snippetDuration,
                            progressText
                            );

                        // Add created audios to dictionary with nestingInfo as key

                        audioCoords.Add(new KeyValuePair<List<int>, List<LangAudio>>(nestingInfo, roundAudios));
                    }
                }
            }

            // Write MixedAudioMetadata JSON file to register information about mixed audios
            var jsonString = JsonSerializer.Serialize(audioCoords, _options);
            File.WriteAllText($"{mixedAudioDirectory}/MixedAudioMetadata.json", jsonString);

            progressText.AppendText("MixedAudioMetadata.json written.");
        }

        /// <summary>
        /// Iterates through audios to add create mixes of different language proportions.
        /// </summary>
        /// 
        /// <remark> 
        /// NOTE: currently only works for 2 languages. Mixing 3+ languages would require iterating pairwise.
        /// </remark>
        /// 
        /// <param name="settings"></param>
        /// <param name="morphSteps"></param>
        /// <param name="femaleAudios"></param>
        /// <param name="maleAudios"></param>
        /// <param name="mixedAudioDirectory"></param>
        /// <param name="audioFolderString"></param>
        /// <param name="nestingInfo"></param>
        /// <param name="startingTimestamp"></param>
        /// <param name="duration"></param>
        /// <param name="progressText"></param>
        /// <returns></returns>
        private static List<LangAudio> WriteAudios
            (
            LangSettings settings, List<List<int>> morphSteps,
            List<LangAudio> femaleAudios, List<LangAudio> maleAudios,
            DirectoryInfo mixedAudioDirectory, string audioFolderString, List<int> nestingInfo,
            TimeSpan startingTimestamp, TimeSpan duration,
            TextBox progressText
            )
        {
            var totalAudios = femaleAudios.Count + maleAudios.Count;
            var lang2FemaleCount = 0;
            var lang2MaleCount = 0;

            List<LangAudio> mixedAudios = [];

            for (int i = 0; i <= totalAudios / 2; i++)
            {
                var lang2Audios = lang2FemaleCount + lang2MaleCount;
                var lang1Audios = totalAudios / 2 - lang2Audios;

                // If current morph step is in the list of morph steps, create audio for it
                List<int> curMorphStep = [
                    lang1Audios * 100 / (totalAudios / 2),
                    lang2Audios * 100 / (totalAudios / 2)
                    ];
                if (morphSteps.Where(morphStep => morphStep.SequenceEqual(curMorphStep)).Any())
                {
                    List<LangAudio> audiosToMix = [];

                    // Select audios according to current language balance
                    SelectAudios(audiosToMix, femaleAudios, maleAudios, lang2FemaleCount, lang2MaleCount, settings.TestLangs);

                    // Create Audio object from audiosToMix and add it to mixedAudios
                    mixedAudios.Add(LangAudioManager.CreateStaticAudioMix(audiosToMix, audioFolderString, startingTimestamp, duration, nestingInfo));

                    progressText.AppendText($"Created static audio mix with audios:\r\n{string.Join(",\r\n", audiosToMix.Select(audio => audio.FileName))}.\r\n");
                }

                // To ensure audios of the same gender are swapped in each iteration, update female count if iterator is even, male if odd
                if (i % 2 == 0) lang2FemaleCount++;
                else lang2MaleCount++;
            }

            progressText.AppendText($"Static audio mixes created for block {nestingInfo[0]}, sequence {nestingInfo[1]}, round {nestingInfo[2]}.\r\n");

            return mixedAudios;
        }

        private static List<List<int>> GetMorphSteps(LangSettings settings)
        {
            if (settings.UseExplicitMorphSteps) return settings.MorphSteps;

            // Check that audios can be evenly distributed for requested number of morphs
            var audioCount = settings.AudioCount;
            var morphCount = settings.MorphCount;
            if ((audioCount - 1) % (morphCount - 1) != 0) ErrorManager.ThrowError($"Requested morph count ({morphCount}) does not result in a balanced distribution for the requested number of audios ({audioCount}). To ensure a balanced distribution, MorphCount-1 has to be a divisor of AudioCount-1 (check settings.json).");

            // Generate morph steps by decrementing lang1 percentage and incrementing lang2 percentage
            List<List<int>> morphSteps = [];
            var stepSize = 100 / (settings.MorphCount - 1);
            for (int i = 0; i < settings.MorphCount; i++)
            {
                var lang2Percentage = i * stepSize;
                morphSteps.Add([100 - lang2Percentage, lang2Percentage]);
            }

            return morphSteps;
        }
        /// <summary>
        /// Updates audiosToMix with a number of audios from each language.
        /// </summary>
        /// <param name="audiosToMix">List of audios to mix</param>
        /// <param name="femaleAudios">List of female audios</param>
        /// <param name="maleAudios">List of male audios</param>
        /// <param name="lang2FemaleCount">Lang 2 female audio count</param>
        /// <param name="lang2MaleCount">Lang 2 male audio count</param>
        /// <param name="testLangs">List of Langs used for testing</param>
        private static void SelectAudios(List<LangAudio> audiosToMix, List<LangAudio> femaleAudios, List<LangAudio> maleAudios, int lang2FemaleCount, int lang2MaleCount, List<Lang> testLangs)
        {
            var lang1FemaleCount = femaleAudios.Count / 2 - lang2FemaleCount;
            var lang1MaleCount = maleAudios.Count / 2 - lang2MaleCount;

            // Randomize audios to take different ones for each stimulus level
            var randomizedFemaleAudios = femaleAudios.ToArray();
            var randomizedMaleAudios = maleAudios.ToArray();
            _rnd.Shuffle(randomizedFemaleAudios);
            _rnd.Shuffle(randomizedMaleAudios);

            // Add lang1 audios
            audiosToMix.AddRange(randomizedFemaleAudios.Where(audio => audio.Lang == testLangs[0]).Take(lang1FemaleCount));
            audiosToMix.AddRange(randomizedMaleAudios.Where(audio => audio.Lang == testLangs[0]).Take(lang1MaleCount));

            // Add lang2 audios
            audiosToMix.AddRange(randomizedFemaleAudios.Where(audio => audio.Lang == testLangs[1]).Take(lang2FemaleCount));
            audiosToMix.AddRange(randomizedMaleAudios.Where(audio => audio.Lang == testLangs[1]).Take(lang2MaleCount));
        }
        private static List<LangAudio> ExtractVoicesByGender(List<LangAudio> audios, VoiceGender gender, List<Lang> testLangs)
        {
            return [.. audios.Where(audio => audio.VoiceGender == gender && audio.Lang.HasValue && testLangs.Contains(audio.Lang.Value))];
        }
        private static TextBox CreateProgressForm()
        {
            Form progressForm = new()
            {
                Text = "Creating Static Audio Mixes",
                Size = new Size(600, 400)
            };

            TextBox progressText = new()
            {
                Text = $"Initializing mix creation.\r\n",
                Multiline = true,
                AcceptsReturn = true,
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
