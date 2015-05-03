using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Hurricane.Model.AudioEngine;

namespace Hurricane.Model.Music.Playable
{
    public abstract class Playable : PropertyChangedBase, IPlayable
    {
        private string _title;
        private string _artist;


        public string Title
        {
            get { return _title; }
            set { SetProperty(value, ref _title); }
        }

        public string Artist
        {
            get { return _artist; }
            set { SetProperty(value, ref _artist); }
        }

        public BitmapImage Cover
        {
            get { throw new NotImplementedException(); }
        }

        public Task<IPlaySource> GetSoundSource()
        {
            throw new NotImplementedException();
        }

        public Task LoadImage()
        {
            throw new NotImplementedException();
        }
    }
}