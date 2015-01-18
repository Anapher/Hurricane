using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hurricane.Music.Data;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.Track
{
    public abstract class WebTrackResultBase : PropertyChangedBase, IRepresentable, IDownloadable, IMusicInformation
    {
        public TimeSpan Duration { get; set; }
        public string Title { get; set; }
        public string Uploader { get; set; }
        public uint Year { get; set; }
        public string ImageUrl { get; set; }
        public int Views { get; set; }
        public abstract ProviderName ProviderName { get; }
        public string Url { get; set; }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                SetProperty(value, ref _image);
            }
        }

        private bool _isLoadingImage;
        public bool IsLoadingImage
        {
            get { return _isLoadingImage; }
            set
            {
                SetProperty(value, ref _isLoadingImage);
            }
        }

        public object Result { get; set; }
        public abstract Task<PlayableBase> ToPlayable();
        public abstract GeometryGroup ProviderVector { get; }

        public IRepresentable Representer { get { return this; } }

        public async Task DownloadImage()
        {
            try
            {
                if (string.IsNullOrEmpty(ImageUrl)) return;
                using (var client = new WebClient { Proxy = null })
                {
                    IsLoadingImage = true;
                    Image = await Utilities.ImageHelper.DownloadImage(client, ImageUrl);
                    IsLoadingImage = false;
                }
            }
            catch
            {
                // ignored
            }   
        }

        private RelayCommand _openUrl;
        public RelayCommand OpenUrl
        {
            get { return _openUrl ?? (_openUrl = new RelayCommand(parameter => { Process.Start(Url); })); }
        }

        public abstract string DownloadParameter { get; }

        public abstract string DownloadFilename { get; }

        public abstract Download.DownloadMethod DownloadMethod { get; }

        public abstract bool CanDownload { get; }

        public string Artist
        {
            get { return Uploader; }
            set
            {
                throw new NotImplementedException();
            }
        }

        public async Task<BitmapImage> GetImage()
        {
            return Image;
        }

        public string Genres { get; set; }

        public string Album { get; set; }
    }

    public enum ProviderName { SoundCloud, YouTube}
}
