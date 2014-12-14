using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundOut;

namespace Hurricane.Music
{
    public class VolumeFading
    {
        public bool IsFading { get; set; }
        private bool _cancel = false;

        protected async Task Fade(float from, float to, TimeSpan duration, bool GetLouder, ISoundOut soundout)
        {
            IsFading = true;
            float different = Math.Abs(to - from);
            float step = different / ((float)duration.TotalMilliseconds / 20);
            float currentvolume = from;

            for (int i = 0; i < duration.TotalMilliseconds / 20; i++)
            {
                if (_cancel) { _cancel = false; break; }
                await Task.Delay(20);
                if (GetLouder) { currentvolume += step; } else { currentvolume -= step; }
                if (currentvolume < 0 || currentvolume > 1) break;
                soundout.Volume = currentvolume;
            }
            IsFading = false;
        }

        public void CancelFading()
        {
            if (!IsFading) return;
            _cancel = true;
        }

        public async Task FadeIn(ISoundOut soundout, float tovolume)
        {
            await Fade(0, tovolume, TimeSpan.FromMilliseconds(300), true, soundout);
        }

        public async Task FadeOut(ISoundOut soundout, float fromvolume)
        {
            await Fade(fromvolume, 0, TimeSpan.FromMilliseconds(300), false, soundout);
        }
    }
}
