using LangBiasTester.Infrastructure.Abstracts;
using LangBiasTester.Infrastructure.Enums;
using LangBiasTester.Infrastructure.Models;
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
                // TODO: find out if can implement ramping in AudioFileReader so ramp is already loaded when playing file
                // TODO: apply RMS adjustment after ramping (RMS always very last step)

                var audioFileReader = new AudioFileReader(audio.FileLocation);
                var waveStream = new OffsetSampleProvider(audioFileReader);
                var waveOut = new WaveOut();

                waveStream.SkipOver = TimeSpan.FromSeconds(audio.SnippetTimestamp);

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
                stream.Value.Dispose();
            }
        }

        // generates stimuli based on mixed audio information (one stimulus per audio mix)
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audios, int stimulusDuration)
        {
            foreach (var audio in audios)
            {
                Stimulus newStimulus = new(audio.MixLangs);
                newStimulus.Audios.Add(audio);
                newStimulus.Audios[0].SnippetDuration = stimulusDuration;
                stimuli.Add(newStimulus);
            }
        }
        // generates stimuli based on explicit ratios given (e.g., 100/0, 75/25, 50/50, 25/75, 0/100)
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration, Lang lang1, Lang lang2, Dictionary<int, int> explicitRatios)
        {
            var lang1List = audioList.Where(audio => (audio.Lang == lang1)).ToList();
            var lang2List = audioList.Where(audio => (audio.Lang == lang2)).ToList();

            var audioCount = Math.Min(lang1List.Count, lang2List.Count);

            foreach (KeyValuePair<int, int> ratio in explicitRatios)
            {
                int lang1ct = audioCount*ratio.Key / 100;
                int lang2ct = audioCount*ratio.Value / 100;

                stimuli.Add(new Stimulus(lang1, lang2, lang1ct, lang2ct));
            }
        }
        // TODO (low-priority): generates stimuli based on the increment in percentage (e.g., incr=25 will generate the same list as above)
        internal static void Init(List<Stimulus> stimuli, List<LangAudio> audioList, int stimulusDuration, Lang lang1, Lang lang2, int percentageIncrement)
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
        //TODO: split audios evenly according to stimuli. Calls ValidateSplit to ensure it can be done.
        internal static void EvenSplit() { }

        //TODO: check if audios can be evenly split given the stimuli
        internal static void ValidateSplit() { }
    }
}
