using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Utils;
using CSCore.DSP;

namespace Hurricane.Music.Visualization
{
    class SampleAnalyser
    {
        private bool isInitialized = false;
        private WaveFormat waveFormat;
        private readonly Complex[] storedSamples;
        private int sampleOffset;
        private float[,] _peaks;

        public SampleAnalyser(int storageSize)
        {
            storedSamples = new Complex[storageSize];
        }

        public void Initialize(WaveFormat waveFormat)
        {
            if (isInitialized)
                throw new InvalidOperationException("Can't reuse SampleAnalyser.");

            if (waveFormat == null)
                throw new ArgumentNullException("waveFormat");

            _peaks = new float[waveFormat.Channels, 2];
            this.waveFormat = waveFormat;
            isInitialized = true;
        }

        public void Add(float left, float right)
        {
            int channels = waveFormat.Channels;
            float[] arr;
            if (channels == 1)
            {
                arr = new float[] { left };
            }
            else
            {
                arr = new float[] { left, right };
            }

            this.Add(arr);
        }

        public void Add(float[] samples)
        {
            CheckForInitialzed();
            if (samples.Length % waveFormat.Channels != 0)
                throw new ArgumentException("Length of samples to add has to be multiple of the channelCount.");

            int channels = waveFormat.Channels;
            int i = 0;
            while (i < samples.Length)
            {
                float s = MergeSamples(samples, i, channels);
                storedSamples[sampleOffset].Real = s;
                storedSamples[sampleOffset].Imaginary = 0f;

                sampleOffset += 1;

                UpdatePeaks(samples, i, channels);

                if (sampleOffset >= storedSamples.Length)
                {
                    sampleOffset = 0;
                }
                i += channels;
            }
        }

        public void CalculateFFT(float[] resultBuffer)
        {
            Complex[] input = new Complex[storedSamples.Length];
            storedSamples.CopyTo(input, 0);

            FastFourierTransformation.Fft(input, Convert.ToInt32(Math.Truncate(Math.Log(storedSamples.Length, 2))), FftMode.Forward);
            for (int i = 0; i <= input.Length / 2 - 1; i++)
            {
                var z = input[i];
                resultBuffer[i] = (float)z.Value;
            }
        }

        private void CheckForInitialzed()
        {
            if (!isInitialized)
                throw new InvalidOperationException("SampleAnalyser is not initialized.");
        }

        private void UpdatePeaks(float[] samples, int index, int channelCount)
        {
            int i = index;
            _peaks[0, 1] = Math.Max(_peaks[0, 1], samples[i]);
            _peaks[0, 0] = Math.Min(_peaks[0, 0], samples[i]);
            if (channelCount == 2)
            {
                _peaks[1, 1] = Math.Max(_peaks[1, 1], samples[i + 1]);
                _peaks[1, 0] = Math.Min(_peaks[1, 0], samples[i + 1]);
            }
            if (channelCount > 2)
            {
                for (int j = 2; j <= channelCount - 1; j++)
                {
                    _peaks[i, 1] = Math.Max(_peaks[i, 1], samples[i + j]);
                    _peaks[i, 0] = Math.Min(_peaks[i, 0], samples[i + j]);
                }
            }
        }

        private float MergeSamples(float[] samples, int index, int channelCount)
        {
            if (channelCount == 1)
            {
                return samples[index];
            }
            if (channelCount == 2)
            {
                return (samples[index] + samples[index + 1]) / 2f;
            }
            else
            {
                float z = 0f;
                for (int i = 0; i <= channelCount - 1; i++)
                {
                    z += samples[index + i];
                }
                return z / 2f;
            }
        }

    }
}
