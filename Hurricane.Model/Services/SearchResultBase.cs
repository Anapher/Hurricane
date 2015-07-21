using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music.Imagment;

namespace Hurricane.Model.Services
{
    public abstract class SearchResultBase : ISearchResult, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; protected set; }
        string ISearchResult.Artist
        {
            get { return Artist; }
            set { Artist = value; }
        }

        ImageProvider ISearchResult.Cover
        {
            get { return Cover; }
            set { Cover = value; }
        }

        string ISearchResult.Title
        {
            get { return Title; }
            set { Title = value; }
        }

        public string Artist { get; protected set; }
        public abstract ImageProvider Cover { get; protected set; }
        public bool IsAvailable { get; } = true;
        public bool IsPlaying { get; set; }
        public object Tag { get; set; }
        public TimeSpan Duration { get; protected set; }

        public abstract Geometry ProviderIcon { get; }
        public abstract string ProviderName { get; }
        public abstract string Url { get; }

        public abstract ConversionInformation ConvertToStreamable();
        public abstract Task<IPlaySource> GetSoundSource();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected async Task<BitmapImage> DownloadImage(string url)
        {
            using (var webClient = new WebClient { Proxy = null })
            {
                using (var mr = new MemoryStream(await webClient.DownloadDataTaskAsync(url)))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = mr;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
        }
    }
}