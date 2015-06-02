using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CSCore.SoundOut;

namespace Hurricane.Model.AudioEngine.Engines
{
    sealed class FadingService
    {
        private const int WaitLength = 20;
        private const int Duration = 300;

        private CancellationTokenSource _cancellationToken;

        public FadingService()
        {
            _cancellationToken = new CancellationTokenSource();
        }

        public bool IsFading { get; set; }

        /// <summary>
        /// Fade the soundOut slowly in
        /// </summary>
        /// <param name="soundOut">The sound out from CSCore</param>
        /// <param name="toVolume">The final volume</param>
        /// <returns></returns>
        public async Task FadeIn(ISoundOut soundOut, float toVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return;

            IsFading = true;
            await Fade(0, toVolume, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
            IsFading = false;
        }

        /// <summary>
        /// Fade the soundOut slowly out
        /// </summary>
        /// <param name="soundOut">The sound out from CSCore</param>
        /// <param name="fromVolume">The final volume</param>
        /// <returns></returns>
        public async Task FadeOut(ISoundOut soundOut, float fromVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return;

            IsFading = true;
            await Fade(fromVolume, 0, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
            IsFading = false;
        }

        /// <summary>
        /// Fade the soundOut slowly out and disposes it after that
        /// </summary>
        /// <param name="soundOut">The sound out from CSCore</param>
        /// <param name="duration">The duration</param>
        /// <returns></returns>
        public async Task CrossfadeOut(ISoundOut soundOut, TimeSpan duration)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return;

            IsFading = true;
            await Fade(soundOut.Volume, 0, duration, soundOut, _cancellationToken.Token);
            IsFading = false;
            if (soundOut.PlaybackState != PlaybackState.Stopped)
                soundOut.Stop();
            using (soundOut) 
                soundOut.WaveSource.Dispose();
        }

        public void Cancel()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        private async static Task Fade(float volumeFrom, float volumeTo, TimeSpan duration, ISoundOut soundOut, CancellationToken token)
        {
            var different = Math.Abs(volumeTo - volumeFrom);
            var durationInt = (int)duration.TotalMilliseconds;
            var stepCount = durationInt/WaitLength;
            var step = different / stepCount;
            var currentVolume = volumeFrom;
            for (int i = 0; i < durationInt / WaitLength; i++)
            {
                try
                {
                    await Task.Delay(WaitLength, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                currentVolume += volumeTo > volumeFrom ? step : -step;
                if (currentVolume < 0 || currentVolume > 1)
                {
                    //It's not exact with the float values. if we would use decimal, this code would never reach. This prevents errors
                    break;
                }

                try
                {
                    soundOut.Volume = currentVolume;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            soundOut.Volume = volumeTo; //Because it won't be exact (because of the float)
        }
    }
}