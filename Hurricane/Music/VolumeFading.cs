using System;
using System.Threading;
using System.Threading.Tasks;
using CSCore.SoundOut;

namespace Hurricane.Music
{
    public class VolumeFading : IDisposable
    {
        public bool IsFading { get; set; }

        private bool _cancel;

        public double OutDuration { get; set; }

        protected AutoResetEvent cancelledwaiter;

        protected async Task Fade(float from, float to, TimeSpan duration, bool getLouder, ISoundOut soundout)
        {
            IsFading = true;
            float different = Math.Abs(to - from);
            float step = different / ((float)duration.TotalMilliseconds / 20);
            float currentvolume = from;

            for (int i = 0; i < duration.TotalMilliseconds / 20; i++)
            {
                if (_cancel) { _cancel = false; OnCancelled(); break; }
                await Task.Delay(20);
                if (getLouder) { currentvolume += step; } else { currentvolume -= step; }
                if (currentvolume < 0 || currentvolume > 1) break;
                try
                {
                    soundout.Volume = currentvolume;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
            IsFading = false;
        }

        #region Cancel
        protected virtual void OnCancelled()
        {
            cancelledwaiter.Set();
        }

        public void WaitForCancel()
        {
            if (IsFading)
            {
                cancelledwaiter.WaitOne(50);
            }
        }

        public void CancelFading()
        {
            if (!IsFading) return;
            _cancel = true;
        }

        #endregion

        #region Public Fading
        public async Task FadeIn(ISoundOut soundOut, float toVolume)
        {
            await Fade(0, toVolume, TimeSpan.FromMilliseconds(300), true, soundOut);
        }

        public async Task FadeOut(ISoundOut soundOut, float fromVolume)
        {
            await Fade(fromVolume, 0, TimeSpan.FromMilliseconds(300), false, soundOut);
        }

        public async void CrossfadeIn(ISoundOut soundOut, float toVolume)
        {
            await Fade(0, toVolume, TimeSpan.FromSeconds(OutDuration), true, soundOut);
        }

        #endregion

        #region Constructor and Deconstructor
        public void Dispose()
        {
            cancelledwaiter.Dispose();
        }

        public VolumeFading()
        {
            cancelledwaiter = new AutoResetEvent(false);
        }
        #endregion
    }

    public class Crossfade
    {
        public bool IsCrossfading { get; set; }
        private bool _cancel;
        public async void FadeOut(double seconds, ISoundOut soundOut)
        {
            IsCrossfading = true;
            var steps = seconds / 0.2;
            var soundstep = soundOut.Volume / (float)steps;

            for (int i = 0; i < steps; i++)
            {
                if (_cancel) { _cancel = false; break; }
                await Task.Delay(200);
                try
                {
                    var value = soundOut.Volume - soundstep;
                    if (0 > value) break;
                    soundOut.Volume -= soundstep;
                }
                catch (ObjectDisposedException)
                {
                    IsCrossfading = false;
                    break;
                }
            }

            IsCrossfading = false;
            if (soundOut.PlaybackState != PlaybackState.Stopped) soundOut.Stop();
            soundOut.WaveSource.Dispose();
            soundOut.Dispose();
        }

        public void CancelFading()
        {
            if (!IsCrossfading) return;
            _cancel = true;
        }
    }
}
