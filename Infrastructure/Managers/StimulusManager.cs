using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Models;
using LangBiasTester.Operators;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester.Infrastructure.Managers
{
    internal class StimulusManager
    {
        internal static Stimulus Clone(Stimulus originalStimulus)
        {
            Stimulus newStimulus = new(originalStimulus.Langs)
            {
                Audios = originalStimulus.Audios
            };

            return newStimulus;
        }
        internal static void RunStimulus(Stimulus stimulus)
        {
            // TODO: Move this to Init() so buffers are ready by the time they need to be played
            // Iterates through all audio snippets in the current trial and plays them simultaneously.
            // If mixed audio is used instead, only one stream will be created.
            foreach (var audio in stimulus.Audios)
            {
                var audioFileReader = new AudioFileReader(audio.FileLocation);
                var waveStream = new OffsetSampleProvider(audioFileReader);
                var waveOut = new WaveOut();

                waveStream.SkipOver = audio.SnippetTimestamp;

                waveOut.Init(waveStream);

                stimulus.WaveStreams.Add(waveStream, waveOut);
            }

            foreach (KeyValuePair<OffsetSampleProvider, WaveOut> stream in stimulus.WaveStreams)
            {
                stream.Value.Play();
            }
        }

        internal static void StopStimulus(Stimulus stimulus)
        {
            foreach (KeyValuePair<OffsetSampleProvider, WaveOut> stream in stimulus.WaveStreams)
            {
                stream.Value.Stop();
            }
        }

        internal static void DisposeOfStimulus(Stimulus stimulus)
        {
            foreach (KeyValuePair<OffsetSampleProvider, WaveOut> stream in stimulus.WaveStreams)
            {
                stream.Value.Dispose();
            }
        }

        internal static void RunMultipleStimuli(List<Stimulus> stimuli, int stimulusDuration, int timeBetweenStimuli)
        {
            foreach (var stimulus in stimuli)
            {
                RunStimulus(stimulus);

                Thread.Sleep(stimulusDuration * 1000);

                StopStimulus(stimulus);

                Thread.Sleep(timeBetweenStimuli * 1000);
            }
        }

        internal static void DisposeOfMultipleStimuli(List<Stimulus> stimuli)
        {
            foreach (var stimulus in stimuli)
            {
                DisposeOfStimulus(stimulus);
            }
        }

        // generates stimuli based on mixed audio information (one stimulus per audio mix)
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration)
        {
            foreach (var audio in audioList)
            {
                Stimulus newStimulus = new(audio.MixLangs);
                newStimulus.Audios.Add(audio);
                newStimulus.Audios[0].SnippetDuration = TimeSpan.FromSeconds(stimulusDuration);
                stimuli.Add(newStimulus);
            }
        }
        // DEPRECATED: generates stimuli based on explicit ratios given (e.g., 100/0, 75/25, 50/50, 25/75, 0/100) (fixing would require implementing the same logic as in Util to select audios and such).
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration, List<Lang> langs, List<List<int>> explicitRatios)
        {
            var ratioTotals = explicitRatios.Select(ratioList => ratioList.Sum());
            if (ratioTotals.Any(sum => sum != 100)) ErrorManager.ThrowError("Not all ratios add up to 100%. Check settings.json.");
            if (explicitRatios.Where(ratio => ratio.Count != langs.Count).Any()) ErrorManager.ThrowError("Missing ratio for some languages. Check settings.json.");

            var langAudios = new Dictionary<Lang, List<LangAudio>>();

            foreach (var lang in langs)
            {
                langAudios.Add(lang, [.. audioList.Where(audio => (audio.Lang == lang))]);
            }

            var audioCounts = langAudios.Values.Select(audioList => audioList.Count);
            var audioCount = audioCounts.Min();

            foreach (var ratio in explicitRatios)
            {
                var langCounts = new Dictionary<Lang, int>();

                for (int i = 0; i < langs.Count; i++)
                {
                    langCounts.Add(langs[i], ratio[i] / 100);
                }

                stimuli.Add(new Stimulus(langCounts));
            }
        }
        // TODO (low-priority): generates stimuli based on the increment in percentage (e.g., incr=25 will generate the same list as comment above)
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration, List<Lang> langs, int percentageIncrement)
        {
            // finds number of audios (validates split), converts from %increment to discrete audio increment and calls Init overload underneath
        }
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration, Lang lang1, Lang lang2, int audioCount, int increment)
        {
            for (int i = 0; i <= audioCount; i += increment)
            {
                stimuli.Add(new Stimulus(lang1, lang2, audioCount-i, i));
            }
        }
    }
}
