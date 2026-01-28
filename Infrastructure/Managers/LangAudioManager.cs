using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class LangAudioManager : AudioManager
    {
        internal LangAudioManager(int seed) : base(seed) { }

        /// <summary>
        /// Creates a static audio mix from a list of audio snippets and saves it as a .wav file.
        /// </summary>
        /// <param name="Audios">The list of files to mix</param>
        /// <param name="folder">Folder to save the .wav file to</param>
        internal static LangAudio CreateStaticAudioMix(List<LangAudio> audiosToMix, string folder, TimeSpan startingTimestamp, TimeSpan duration, List<int> nestingInfo)
        {
            var langMixes = new Dictionary<Lang, int>();
            var fileName = GenerateFileName(audiosToMix, langMixes, nestingInfo);
            var fileLocation = new FileInfo($"{folder}/{fileName}.wav").FullName;

            SaveMixedAudio(audiosToMix, $"{folder}/{fileName}.wav", startingTimestamp, duration);

            // Create audio object to add mixed audios
            LangAudio mixedAudio = new()
            {
                FileName = fileName,
                FileLocation = fileLocation,
                Duration = new AudioFileReader(fileLocation).TotalTime,
                MixLangs = langMixes,
                RawAudios = [.. audiosToMix.Select(audio => audio.FileName)]
            };

            return mixedAudio;
        }

        /// <summary>
        /// Mixes the audios and saves it to disk.
        /// </summary>
        /// <param name="audiosToMix">A list of <see cref="Audio"/> objects with the information about the audios to mix.</param>
        /// <param name="fileName">The name and location of the file.</param>
        private static void SaveMixedAudio(List<LangAudio> audiosToMix, string fileName, TimeSpan startingTimestamp, TimeSpan duration)
        {
            // Create a list for the AudioFileReaders for each file
            Dictionary<AudioFileReader, float[]> audioStreams = [];

            // Read each file and add it to the list
            foreach (var audio in audiosToMix)
            {
                var audioFile = new AudioFileReader(audio.FileLocation);
                audioStreams[audioFile] = ReadAllSamples(new AudioFileReader(audio.FileLocation));
            }

            // Normalize audios
            var rmsNormalizedAudios = NormalizeRMS(audioStreams);

            // Trim audios to desired snippet
            var offsetProviders = rmsNormalizedAudios.Select(audio => new OffsetSampleProvider(audio)
            {
                SkipOver = startingTimestamp,
                Take = duration
            });
            
            // TODO: Add ramping
            // cosine ramping logic

            // Mix audios
            var mixer = new MixingSampleProvider(offsetProviders);

            // Write mixed audio to file
            WaveFileWriter.CreateWaveFile16(fileName, mixer);

            foreach (var reader in audioStreams.Keys) reader.Dispose();
        }

        /// <summary>
        /// Generates a name for the file to be saved based on which languages were used and how many audios of each.
        /// </summary>
        /// <param name="audiosToMix">The list of audios mixed.</param>
        /// <param name="langMixes">The specific mix of languages.</param>
        /// <returns>A string corresponding to the name of the file.</returns>
        private static string GenerateFileName(List<LangAudio> audiosToMix, Dictionary<Lang, int> langMixes, List<int> nestingInfo)
        {
            var audioLangs = audiosToMix.Select(audio => audio.Lang).Distinct().ToList();
            List<string> langCounts = [];

            foreach (Lang lang in audioLangs.Select(item => (Lang) item))
            {
                var langAudioCount = audiosToMix.Count(audio => audio.Lang == lang);
                langMixes.Add(lang, langAudioCount);
                langCounts.Add($"{lang}{langAudioCount}");
            }

            var langCountString = string.Join("_", langCounts);
            var nestingString = string.Join("_", nestingInfo.Select(number => number.ToString()));

            return string.Join("_", langCountString, nestingString);
        }

        /// <summary>
        /// Selects audios meeting the given stimulus, trims them to the specified snippet duration, and selects random timestamps for each snippet.
        /// </summary>
        /// <param name="audios">The list of audios to mix</param>
        /// <param name="stimulus">The stimulus specifying how many audios for each language are to be mixed</param>
        /// <param name="snippetDuration">The duration of the snippets to be trimmed</param>
        /// <returns></returns>
        internal List<LangAudio> PrepareMix(List<LangAudio> audios, Stimulus stimulus, int snippetDuration)
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
        private List<LangAudio> SelectAudiosMeetingStimulus(List<LangAudio> audios, Stimulus stimulus)
        {
            List<LangAudio> selectedAudios = [];

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
        /// Adjusts the snippet duration for each <see cref="LangAudio"/> object in the given collection.
        /// </summary>
        /// <param name="audios">A list of <see cref="LangAudio"/> objects whose snippet durations will be updated.</param>
        /// <param name="snippetDuration">The duration, in seconds, to set for each audio snippet. Must be a positive value.</param>
        protected static void TrimAudioSnippets(List<LangAudio> audios, int snippetDuration)
        {
            foreach (var audio in audios)
            {
                audio.SnippetDuration = TimeSpan.FromSeconds(snippetDuration);
            }
        }

        /// <summary>
        /// Selects a random timestamp for each audio object in the provided list, assigns it to the 
        /// <c>SnippetTimestamp</c> property of each audio object.
        /// </summary>
        /// <param name="audios">A list of <see cref="LangAudio"/> objects for which random snippet timestamps will be selected.</param>
        /// <param name="snippetDuration">The duration of the snippet, in seconds. Must be less than or equal to the duration of each audio.</param>
        protected void SelectRandomTimestamp(List<LangAudio> audios, int snippetDuration)
        {
            foreach (var audio in audios)
            {
                var duration = audio.Duration.TotalSeconds;
                var maxStart = duration - snippetDuration;

                audio.SnippetTimestamp = TimeSpan.FromSeconds(_random.Next(0, (int)maxStart));
            }
        }
    }
}
