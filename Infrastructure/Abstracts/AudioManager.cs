using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace LangBiasTester.Infrastructure.Abstracts
{
    internal class AudioManager
    {
        protected readonly Random _random;
        internal AudioManager(int seed) => _random = new Random(seed);

        /// <summary>
        /// Calculates the Root Mean Square for an audio sample vector.
        /// </summary>
        /// <param name="audioArray">
        /// The audio sample vector.
        /// </param>
        /// <returns>
        /// The root mean square of the vector.
        /// </returns>
        protected static float GetRMS(float[] audioArray)
        {
            var sumOfSquares = 0f;

            for (int i = 0; i < audioArray.Length; i++)
            {
                var num = audioArray[i];
                sumOfSquares += num * num;
            }

            var rms = float.Sqrt(sumOfSquares / audioArray.Length);

            return rms;
        }

        /// <summary>
        /// Normalizes the RMS of the audios to mix
        /// </summary>
        /// <param name="audios">The audios to mix, along with the array corresponding to its full sample.</param>
        /// <returns>
        /// A <see cref="VolumeSampleProvider"/> object containing the audio with its volume normalized.
        /// </returns>
        protected static VolumeSampleProvider[] NormalizeRMS(Dictionary<AudioFileReader, float[]> audios)
        {
            var normalizedAudios = new VolumeSampleProvider[audios.Count];
            var rmsValues = new float[audios.Count]; 
            var minRms = float.MaxValue;

            int i = 0;
            foreach (var (afr, stream) in audios)
            {
                var provider = new VolumeSampleProvider(afr);
                var rms = GetRMS(stream);

                normalizedAudios[i] = provider;
                rmsValues[i] = rms;

                if (rms < minRms) minRms = rms;

                i++;
            }

            for (int j = 0; j < audios.Count; j++)
            {
                normalizedAudios[j].Volume *= minRms/rmsValues[j];
            }

            return normalizedAudios;
        }

        protected static float[] CosRamp(float[] audioArray, double samplingRate, double rampDuration = 0.01)
        {
            var output = new float[audioArray.Length];
            var audioSpan = new Span<float>(audioArray);
            var target = new Span<float>(output);

            // gate = cos((rampDuration * samplingRate) evenly spaced points from pi to 2pi)
            var bound = (int) (rampDuration * samplingRate);
            var step = Math.PI / (rampDuration * samplingRate);

            var gate = target[..bound];
            var sustain = target[bound..(audioArray.Length - (2*bound))];
            var offset = target[(audioArray.Length - (2*bound))..];

            // multiply each item in gate by cosine weight, leave middle part (sustain) untouched, multiply each item in offset by cosine weight in reverse order
            for (int i = 0; i < bound; i++)
            {
                // get range from pi to 2pi, normalize from [ -1 , 1 ] to [ 0 , 1 ]
                var weight = (float)(0.5 * (Math.Cos(Math.PI + (i * step)) + 1.0));
                gate[i] = audioArray[i] * weight;
                var inv = bound - 1 - i;
                offset[inv] = audioArray[inv] * weight;
            }

            audioSpan.Slice(bound, sustain.Length).CopyTo(sustain);

            return audioSpan.ToArray();
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
        protected static float[] ReadAllSamples(AudioFileReader audio)
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
    }
}
