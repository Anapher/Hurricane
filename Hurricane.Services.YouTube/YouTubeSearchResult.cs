using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model.AudioEngine;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Services;

namespace Hurricane.Services.YouTube
{
    public class YouTubeSearchResult : SearchResultBase
    {
        private readonly string _videoId;

        public YouTubeSearchResult(string title, string artist, string uploader, string videoId, TimeSpan duration, string imageUrl)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
            Cover = new OnlineImage(imageUrl);
            Uploader = uploader;
            _videoId = videoId;
            
        }

        public override sealed ImageProvider Cover { get; protected set; }

        public override string Uploader { get; }
        public override Geometry ProviderIcon => YouTubeService.GetYouTubeVector();
        public override string ProviderUrl { get; } = "https://www.youtube.com/";
        public override string ProviderName { get; } = "YouTube";
        public override string Url => $"https://www.youtube.com/watch?v={_videoId}";

        public override ConversionInformation ConvertToStreamable()
        {
            var youTubeTrack = new YouTubeTrack
            {
                Duration = Duration,
                Title = Title,
                Uploader = Uploader,
                YouTubeVideoId = _videoId
            };

            return new ConversionInformation(youTubeTrack, Artist, null);
        }

        public override Task<IPlaySource> GetSoundSource()
        {
            return YouTubeExtractor.GetPlaySource(_videoId);
        }
    }
}