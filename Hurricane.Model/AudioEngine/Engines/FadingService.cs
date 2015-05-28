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

        public bool IsFading { get; set; }

        public Task FadeIn(ISoundOut soundOut, float toVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return Task.FromResult(0);

            _cancellationToken = new CancellationTokenSource();
            return Fade(0, toVolume, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
        }

        public Task FadeOut(ISoundOut soundOut, float fromVolume)
        {
            if (IsFading && !_cancellationToken.IsCancellationRequested)
                return Task.FromResult(0);

            _cancellationToken = new CancellationTokenSource();
            return Fade(fromVolume, 0, TimeSpan.FromMilliseconds(Duration), soundOut, _cancellationToken.Token);
        }

        public void Cancel()
        {
            _cancellationToken?.Cancel();
        }

        private async Task Fade(float volumeFrom, float volumeTo, TimeSpan duration, ISoundOut soundOut, CancellationToken token)
        {
            if (IsFading) return;
            IsFading = true;

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
            IsFading = false;
        }
    }
}