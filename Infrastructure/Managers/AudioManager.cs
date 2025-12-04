using LangBiasTester.Infrastructure.Models;
using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class AudioManager
    {
        private readonly Random _random;
        internal AudioManager(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Creates a static audio mix from a list of audio snippets and saves it as a .wav file.
        /// </summary>
        /// <param name="Audios">The list of files to mix</param>
        /// <param name="folder">Folder to save the .wav file to</param>
        internal static Audio CreateStaticAudioMix(List<Audio> audiosToMix, string folder)
        {
            var langMixes = new Dictionary<Lang, int>();
            var fileName = GenerateFileName(audiosToMix, langMixes);
            var fileLocation = new FileInfo($"{folder}/{fileName}.wav").FullName;

            SaveMixedAudio(audiosToMix, $"{folder}/{fileName}.wav");

            // Create audio object to add mixed audios
            Audio mixedAudio = new()
            {
                FileName = fileName,
                FileLocation = fileLocation,
                Duration = new AudioFileReader(fileLocation).TotalTime,
                MixLangs = langMixes
            };

            return mixedAudio;
        }

        /// <summary>
        /// Mixes the audios and saves it to disk.
        /// </summary>
        /// <param name="audiosToMix">A list of <see cref="Audio"/> objects with the information about the audios to mix.</param>
        /// <param name="fileName">The name and location of the file.</param>
        private static void SaveMixedAudio(List<Audio> audiosToMix, string fileName)
        {
            // Create a list for the AudioFileReaders for each file
            Dictionary<AudioFileReader, float[]> audioStreams = [];

            // Read each file and add it to the list
            foreach (var audio in audiosToMix)
            {
                var audioFile = new AudioFileReader(audio.FileLocation);
                audioStreams[audioFile] = ReadAllSamples(new AudioFileReader(audio.FileLocation));
            }

            // Normalize and mix audios
            var rmsNormalizedAudios = NormalizeRMS(audioStreams);
            var mixer = new MixingSampleProvider(rmsNormalizedAudios);

            WaveFileWriter.CreateWaveFile16(fileName, mixer);

            foreach (AudioFileReader reader in audioStreams.Keys)
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Generates a name for the file to be saved based on which languages were used and how many audios of each.
        /// </summary>
        /// <param name="audiosToMix">The list of audios mixed.</param>
        /// <param name="langMixes">The specific mix of languages.</param>
        /// <returns>A string corresponding to the name of the file.</returns>
        private static string GenerateFileName(List<Audio> audiosToMix, Dictionary<Lang, int> langMixes)
        {
            var audioLangs = audiosToMix.Select(audio => audio.Lang).Distinct().ToList();
            List<string> langCountString = [];

            foreach (Lang lang in audioLangs.Select(item => (Lang)item))
            {
                var langAudioCount = audiosToMix.Count(audio => audio.Lang == lang);
                langMixes.Add(lang, langAudioCount);
                langCountString.Add($"{lang}{langAudioCount}");
            }

            return string.Join("", langCountString);
        }

        /// <summary>
        /// Calculates the Root Mean Square for an audio sample vector.
        /// </summary>
        /// <param name="audioArray">
        /// The audio sample vector.
        /// </param>
        /// <returns>
        /// The root mean square of the vector.
        /// </returns>
        private static float GetRMS(float[] audioArray)
        {
            var sumOfSquares = audioArray.Select(num => num*num).Sum();
            var rms = float.Sqrt(sumOfSquares/audioArray.Length);

            return rms;
        }

        /// <summary>
        /// Normalizes the RMS of the audios to mix
        /// </summary>
        /// <param name="audios">The audios to mix, along with the array corresponding to its full sample.</param>
        /// <returns>
        /// A <see cref="VolumeSampleProvider"/> object containing the audio with its volume normalized.
        /// </returns>
        private static List<VolumeSampleProvider> NormalizeRMS(Dictionary<AudioFileReader, float[]> audios)
        {
            var rmsValues = audios.Values.Select(sample => GetRMS(sample));
            var minRms = rmsValues.Min();
            var adjustedRmsValues = rmsValues.Select(rms => minRms/rms).ToList();

            var normalizedAudios = audios.Keys.Select(audio => new VolumeSampleProvider(audio)).ToList();

            for (int i = 0; i < audios.Count; i++)
            {
                normalizedAudios[i].Volume *= adjustedRmsValues[i];
            }

            return normalizedAudios;
        }

        /// <summary>
        /// Reads an AudioFileReader audio, commiting its sample to memory as a vector.
        /// </summary>
        /// <param name="audio">
        /// The AudioFileReader object instantiated from the original audio file.
        /// </param>
        /// <returns>
        /// The vector of floats representing the audio.
        /// </returns>
        private static float[] ReadAllSamples(AudioFileReader audio)
        {
            List<float> samples = [];

            var samplesRead = 1;

            while (samplesRead > 0)
            {
                float[] tempBuffer = new float[8192];

                samplesRead = audio.Read(tempBuffer, 0, tempBuffer.Length);

                ArraySegment<float> segment = samplesRead > 0 ? new(tempBuffer, 0, samplesRead) : [];

                samples.AddRange(segment);
            }
            
            return [.. samples];
        }

        /// <summary>
        /// Selects audios meeting the given stimulus, trims them to the specified snippet duration, and selects random timestamps for each snippet.
        /// </summary>
        /// <param name="audios">The list of audios to mix</param>
        /// <param name="stimulus">The stimulus specifying how many audios for each language are to be mixed</param>
        /// <param name="snippetDuration">The duration of the snippets to be trimmed</param>
        /// <returns></returns>
        internal List<Audio> PrepareMix(List<Audio> audios, Stimulus stimulus, int snippetDuration)
        {
            var selectedAudios = SelectAudiosMeetingStimulus(audios, stimulus);
            TrimAudioSnippets(selectedAudios, snippetDuration);
            SelectRandomTimestamp(selectedAudios, snippetDuration);

            return selectedAudios;
        }

        /// <summary>
        /// Selects audios from the provided list that meet the specified stimulus.
        /// </summary>
        /// <param name="audios">List of audios from which to select</param>
        /// <param name="stimulus">The stimulus specifying how many audios for each language are to be mixed</param>
        /// <returns></returns>
        private List<Audio> SelectAudiosMeetingStimulus(List<Audio> audios, Stimulus stimulus)
        {
            List<Audio> selectedAudios = [];

            foreach (KeyValuePair<Lang, int> langEntry in stimulus.Langs)
            {
                var randomizedAudios = audios;
                _random.Shuffle(randomizedAudios.ToArray());

                var filteredAudios = randomizedAudios.Where(audio => audio.Lang == langEntry.Key).Take(langEntry.Value).ToList();
                selectedAudios.AddRange(filteredAudios);
            }

            return selectedAudios;
        }

        /// <summary>
        /// Adjusts the snippet duration for each <see cref="Audio"/> object in the given collection.
        /// </summary>
        /// <param name="audios">A list of <see cref="Audio"/> objects whose snippet durations will be updated.</param>
        /// <param name="snippetDuration">The duration, in seconds, to set for each audio snippet. Must be a positive value.</param>
        private static void TrimAudioSnippets(List<Audio> audios, int snippetDuration)
        {
            foreach (Audio audio in audios)
            {
                audio.SnippetDuration = snippetDuration;
            }
        }

        /// <summary>
        /// Selects a random timestamp for each audio object in the provided list, assigns it to the 
        /// <c>SnippetTimestamp</c> property of each audio object.
        /// </summary>
        /// <param name="audios">A list of <see cref="Audio"/> objects for which random snippet timestamps will be selected.</param>
        /// <param name="snippetDuration">The duration of the snippet, in seconds. Must be less than or equal to the duration of each audio.</param>
        private void SelectRandomTimestamp(List<Audio> audios, int snippetDuration)
        {
            foreach (Audio audio in audios)
            {
                var duration = audio.Duration.TotalSeconds;
                var maxStart = duration - snippetDuration;

                audio.SnippetTimestamp = _random.Next(0, (int)maxStart); 
            }
        }
    }
}
