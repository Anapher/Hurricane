using System;
using System.IO;
using System.Threading.Tasks;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public class LocalPlayable : PlayableBase
    {
        private string _trackPath;

        public string TrackPath
        {
            get { return _trackPath; }
            set
            {
                _trackPath = value;
                if (value != null)
                    Extension = Path.GetFileNameWithoutExtension(value).ToUpper();
            }
        }

        public string Extension { get; set; }
        public override bool IsAvailable => File.Exists(TrackPath);

        public override Task<IPlaySource> GetSoundSource()
        {
            return Task.Run(() => (IPlaySource) (new LocalFilePlaySource(TrackPath)));
        }

        public async override Task LoadImage()
        {
            
        }
    }
}