using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Model.Music.Playable;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.AudioEngine.Engines;

namespace Hurricane.Model.Music
{
    public class MusicManager : PropertyChangedBase
    {
        private IPlayable _currentTrack;

        public MusicManager()
        {
            AudioEngine = new CSCoreEngine();

        }

        public IPlayable CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(value, ref _currentTrack); }
        }

        private IAudioEngine AudioEngine { get; set; }
    }
}