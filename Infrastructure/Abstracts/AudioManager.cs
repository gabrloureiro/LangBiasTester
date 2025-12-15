using LangBiasTester.Infrastructure.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

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
        private static float GetRMS(float[] audioArray)
        {
            var sumOfSquares = audioArray.Select(num => num * num).Sum();
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
        protected static List<VolumeSampleProvider> NormalizeRMS(Dictionary<AudioFileReader, float[]> audios)
        {
            var rmsValues = audios.Values.Select(sample => GetRMS(sample));
            var minRms = rmsValues.Min();
            var adjustedRmsValues = rmsValues.Select(rms => minRms / rms).ToList();

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
