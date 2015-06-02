using System;
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

        public async Task FadeIn(ISoundOut soundOut, float toVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return;

            IsFading = true;
            await Fade(0, toVolume, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
            IsFading = false;
        }

        public async Task FadeOut(ISoundOut soundOut, float fromVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return;

            IsFading = true;
            await Fade(fromVolume, 0, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
            IsFading = false;
        }

        public void Cancel()
        {
            _cancellationToken?.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        public async Task CrossfadeOut(ISoundOut soundOut, TimeSpan duration)
        {
            await Fade(soundOut.Volume, 0, duration, soundOut, _cancellationToken.Token);
            if(soundOut.PlaybackState != PlaybackState.Stopped)
                soundOut.Stop();
            using (soundOut)
                soundOut.WaveSource.Dispose();
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