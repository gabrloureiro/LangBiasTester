using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class AudioManager
    {
        /// <summary>
        /// Creates a static audio mix from a list of audio snippets and saves it as a .wav file.
        /// </summary>
        /// <param name="Audios">The list of files to mix</param>
        /// <param name="folder">Folder to save the .wav file to</param>
        internal static Audio CreateStaticAudioMix(List<Audio> AudiosToMix, string folder, TextBox progressBox)
        {
            Audio mixedAudio = new()
            {
                MixLangs = []
            };

            List<AudioFileReader> audioFiles = [];
            var combiningAudios = "";

            foreach (Audio audio in AudiosToMix)
            {
                audioFiles.Add(new AudioFileReader(audio.FileLocation));
                combiningAudios += $"{audio.FileName}, ";
            }

            combiningAudios = combiningAudios.TrimEnd(' ', ',');

            var mixer = new MixingSampleProvider(audioFiles);

            var audioLangs = AudiosToMix.Select(audio => audio.Lang).Distinct().ToList();
            var langCountString = "";

            foreach (Lang lang in audioLangs)
            {
                var langAudioCount = AudiosToMix.Count(audio => audio.Lang == lang);

                mixedAudio.MixLangs.Add(lang, langAudioCount);

                langCountString += $"{lang}{langAudioCount}";
            }

            mixedAudio.FileName = langCountString;

            WaveFileWriter.CreateWaveFile16($"{folder}/{langCountString}.wav", mixer);

            mixedAudio.FileLocation = new FileInfo($"{folder}/{langCountString}.wav").FullName;
            mixedAudio.Duration = new AudioFileReader(mixedAudio.FileLocation).TotalTime;

            foreach (AudioFileReader reader in audioFiles)
            {
                reader.Dispose();
            }

            var progressText = $"Created static audio mix with audios: {combiningAudios}.";

            progressBox.AppendText(Environment.NewLine);
            progressBox.AppendText(progressText);

            return mixedAudio;
        }

        /// <summary>
        /// Selects audios meeting the given condition, trims them to the specified snippet duration, and selects random timestamps for each snippet.
        /// </summary>
        /// <param name="audios">The list of audios to mix</param>
        /// <param name="condition">The condition specifying how many audios for each language are to be mixed</param>
        /// <param name="snippetDuration">The duration of the snippets to be trimmed</param>
        /// <returns></returns>
        internal List<Audio> PrepareMix(List<Audio> audios, Condition condition, int snippetDuration)
        {
            var selectedAudios = SelectAudiosMeetingCondition(audios, condition);
            TrimAudioSnippets(selectedAudios, snippetDuration);
            SelectRandomTimestamp(selectedAudios, snippetDuration);

            return selectedAudios;
        }

        /// <summary>
        /// Selects audios from the provided list that meet the specified condition.
        /// </summary>
        /// <param name="audios">List of audios from which to select</param>
        /// <param name="condition">The condition specifying how many audios for each language are to be mixed</param>
        /// <returns></returns>
        private List<Audio> SelectAudiosMeetingCondition(List<Audio> audios, Condition condition)
        {
            List<Audio> selectedAudios = [];

            Random random = new();

            foreach (KeyValuePair<Lang, int> langEntry in condition.Langs)
            {
                var randomizedAudios = audios;
                random.Shuffle(randomizedAudios.ToArray());

                var filteredAudios = randomizedAudios.Where(audio => audio.Lang == langEntry.Key).Take(langEntry.Value).ToList();
                selectedAudios.AddRange(filteredAudios);
            }

            return selectedAudios;
        }

        /// <summary>
        /// Adjusts the snippet duration for a collection of audio objects.
        /// </summary>
        /// <remarks>This method iterates through the provided list of audio objects and updates the
        /// <c>SnippetDuration</c> property of each object to the specified value.</remarks>
        /// <param name="audios">A list of <see cref="Audio"/> objects whose snippet durations will be updated.</param>
        /// <param name="snippetDuration">The duration, in seconds, to set for each audio snippet. Must be a positive value.</param>
        private void TrimAudioSnippets(List<Audio> audios, int snippetDuration)
        {
            foreach (Audio audio in audios)
            {
                audio.SnippetDuration = snippetDuration;
            }
        }

        /// <summary>
        /// Selects a random timestamp for each audio object in the provided list, ensuring the timestamp falls within
        /// the valid range based on the audio's duration and the specified snippet duration.
        /// </summary>
        /// <remarks>The method calculates a valid range for the snippet's starting timestamp based on the
        /// audio's total duration and the specified snippet duration. A random timestamp within this range is then
        /// assigned to the <see cref="Audio.SnippetTimestamp"/> property of each audio object.</remarks>
        /// <param name="audios">A list of <see cref="Audio"/> objects for which random snippet timestamps will be selected.</param>
        /// <param name="snippetDuration">The duration of the snippet, in seconds. Must be less than or equal to the duration of each audio.</param>
        private void SelectRandomTimestamp(List<Audio> audios, int snippetDuration)
        {
            var random = new Random();

            foreach (Audio audio in audios)
            {
                var duration = audio.Duration.TotalSeconds;
                var maxStart = duration - snippetDuration;

                audio.SnippetTimestamp = random.Next(0, (int)maxStart); 
            }
        }
    }
}
