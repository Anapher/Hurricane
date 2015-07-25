using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hurricane.Model.Data.SqlTables;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetArtistInfo;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetTopTracks;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchArtist;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchTrack;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Music.TrackProperties;
using Hurricane.Utilities;
using Newtonsoft.Json;
using Artist = Hurricane.Model.Music.TrackProperties.Artist;
using ImageProvider = Hurricane.Model.Music.Imagment.ImageProvider;

namespace Hurricane.Model.DataApi
{
    public class LastfmApi
    {
        private static readonly Dictionary<string, OnlineImage> ImageCache = new Dictionary<string, OnlineImage>();

        public LastfmApi(ArtistProvider artists)
        {
            Artists = artists;
            ArtistKeywords = new Dictionary<Artist, List<string>>();
        }

        public ArtistProvider Artists { get; }
        private Dictionary<Artist, List<string>> ArtistKeywords { get; }

        public async Task<Artist> SearchArtistOnline(string name)
        {
            Artist foundArtist;
            if (SearchArtist(name, out foundArtist))
                return foundArtist;

            using (var wc = new WebClient { Proxy = null })
            {
                ArtistSearchResult searchedArtists ;

                var s = (await
                    wc.DownloadStringTaskAsync(
                        $"http://ws.audioscrobbler.com/2.0/?method=artist.search&artist={Uri.EscapeDataString(name)}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"))
                    .FixJsonString();
                try
                {
                    searchedArtists = JsonConvert.DeserializeObject<ArtistSearchResult>(s);
                }
                catch (JsonException)
                {
                    return null;
                }

                var match = searchedArtists?.results?.ArtistMatches?.artist;
                if (match == null)
                    return null;

                if (SearchArtist(match.name, out foundArtist))
                {
                    if (ArtistKeywords.ContainsKey(foundArtist))
                        ArtistKeywords[foundArtist].Add(name);
                    else
                        ArtistKeywords.Add(foundArtist, new List<string> {name});
                    
                    return foundArtist;
                }

                var artist = new Artist(match.name)
                {
                    MusicBrainzId = match.mbid,
                    Url = match.url
                };
                SetImages(artist, match.image);

                await Artists.AddArtist(artist);
                ArtistKeywords.Add(artist, new List<string> { name });
                return artist;
            }
        }

        public async Task SetAdvancedInfoAboutArtist(Artist artist, CultureInfo culture)
        {
            if (artist.ProvidesAdvancedInfo)
                return;

            using (var wc = new WebClient {Proxy = null})
            {
                GetArtistInfoResult artistInfo;
                try
                {
                    var jsonString = (await
                        wc.DownloadStringTaskAsync(string.IsNullOrEmpty(artist.MusicBrainzId)
                            ? $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={Uri.EscapeDataString(artist.Name)}&autocorrect=1&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"
                            : $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={artist.MusicBrainzId}&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"))
                        .FixJsonString();
                    artistInfo = JsonConvert.DeserializeObject<GetArtistInfoResult>(jsonString);
                }
                catch (JsonException)
                {
                    return;
                }

                if (string.IsNullOrEmpty(artist.MusicBrainzId))
                    artist.MusicBrainzId = artistInfo.artist.mbid;

                var topTracksTask =
                    wc.DownloadStringTaskAsync(string.IsNullOrEmpty(artist.MusicBrainzId)
                        ? $"http://ws.audioscrobbler.com/2.0/?method=artist.gettoptracks&artist={artist.Name}&api_key={SensitiveInformation.LastfmKey}&limit=10&format=json"
                        : $"http://ws.audioscrobbler.com/2.0/?method=artist.gettoptracks&mbid={artist.MusicBrainzId}&api_key={SensitiveInformation.LastfmKey}&limit=10&format=json");

                artist.Biography = Regex.Match(artistInfo.artist.bio.summary.Trim(), @"^(?<content>(.+))<a", RegexOptions.Singleline).Groups["content"].Value.Trim();
                if (string.IsNullOrWhiteSpace(artist.Biography))
                    artist.Biography = null;

                if (artistInfo.artist?.similar?.artist != null)
                {
                    var similarArtists = new List<Artist>();
                    foreach (var similarArtist in artistInfo.artist.similar.artist)
                    {
                        Artist existingArtist;
                        if (SearchArtist(similarArtist.name, out existingArtist))
                        {
                            similarArtists.Add(existingArtist);
                            break;
                        }

                        var newArtist = new Artist(similarArtist.name)
                        {
                            Url = similarArtist.url
                        };

                        SetImages(newArtist, similarArtist.image);
                        await Artists.AddArtist(newArtist);
                        similarArtists.Add(newArtist);
                    }
                    artist.SimilarArtists = similarArtists;
                }

                GetTopTracksResult topTrackResult;
                try
                {
                    topTrackResult =
                        JsonConvert.DeserializeObject<GetTopTracksResult>((await topTracksTask).FixJsonString());
                }
                catch (JsonException)
                {
                    return;
                }
                artist.TopTracks = topTrackResult.toptracks?.track?.Where(x => !string.IsNullOrWhiteSpace(x.duration)).Select(x => new TopTrack
                {
                    Artist = artist,
                    Duration = TimeSpan.FromMilliseconds(int.Parse(x.duration)),
                    MusicbrainzId = x.mbid,
                    Name = x.name,
                    Url = x.url,
                    Thumbnail =
                        x.image?.Count > 0
                            ? GetBestImage(x.image)
                            : null
                }).ToList();
            }
            artist.ProvidesAdvancedInfo = true;
        }

        public async Task<Artist> GetArtistByMusicbrainzId(string musicbrainzId, CultureInfo culture)
        {
            using (var wc = new WebClient {Proxy = null})
            {
                var artistInfo =
                    JsonConvert.DeserializeObject<GetArtistInfoResult>(
                        (await
                            wc.DownloadStringTaskAsync(
                                $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={musicbrainzId}&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"))
                            .FixJsonString());
                var artist = new Artist(artistInfo.artist.name)
                {
                    MusicBrainzId = artistInfo.artist.mbid,
                    Url = artistInfo.artist.url
                };
                SetImages(artist, artistInfo.artist.image);
                return artist;
            }
        }

        public async Task<TrackInformation> GetTrackInformation(string title, string artist)
        {
            using (var wc = new WebClient {Proxy = null})
            {
                var s = (await
                    wc.DownloadStringTaskAsync(artist == null
                        ? $"http://ws.audioscrobbler.com/2.0/?method=track.search&track={title}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"
                        : $"http://ws.audioscrobbler.com/2.0/?method=track.search&track={title}&artist={artist}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"));

                var fixedString = s.FixJsonString();
                SearchTrackResult result;
                try
                {
                    result = JsonConvert.DeserializeObject<SearchTrackResult>(fixedString);
                }
                catch (JsonException)
                {
                    return null;
                }

                var trackResult = result?.results?.Trackmatches?.track;
                if (trackResult != null)
                {
                    OnlineImage image = null;
                    if (trackResult.image?.Count > 0)
                    {
                        var imageUrl = trackResult.image.Last().text;

                        if (ImageCache.ContainsKey(imageUrl))
                            image = ImageCache[imageUrl];
                        else
                        {
                            image = new OnlineImage(imageUrl);
                            ImageCache.Add(imageUrl, image);
                        }
                    }

                    return new TrackInformation
                    {
                        Artist = trackResult.artist,
                        Name = trackResult.name,
                        Url = trackResult.url,
                        MusicBrainzId = trackResult.mbid,
                        CoverImage = image
                    };
                }
                return null;
            }
        }

        public bool SearchArtist(string name, out Artist artist)
        {
            var foundArtists =
                Artists.ArtistDictionary.Where(
                    x => string.Equals(x.Value.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (foundArtists.Count > 0)
            {
                artist = foundArtists[0].Value;
                return true;
            }

            var alreadyExistingArtist = ArtistKeywords.FirstOrDefault(x => x.Value.Contains(name)).Key;
            if (alreadyExistingArtist != null)
            {
                artist = alreadyExistingArtist;
                return true;
            }

            artist = null;
            return false;
        }

        private static void SetImages(Artist artist, List<Image> images)
        {
            if (images == null || images.Count == 0) return;
            images = images.Where(x => !string.IsNullOrEmpty(x.text)).ToList();
            if (images.Count == 0)
                return;

            artist.SmallImage = GetSmallImage(images);
            artist.MediumImage = GetMediumImage(images);
            artist.LargeImage = GetBestImage(images);
        }

        private static OnlineImage GetBestImage(IEnumerable<Image> images)
        {
            var imageUrl = images.OrderBy(x => x.size).Last().text;
            if (ImageCache.ContainsKey(imageUrl))
                return ImageCache[imageUrl];

            var onlineImage = new OnlineImage(imageUrl);
            ImageCache.Add(imageUrl, onlineImage);
            return onlineImage;
        }

        private static OnlineImage GetMediumImage(IEnumerable<Image> images)
        {
            var source = images.OrderBy(x => x.size);
            var imageUrl = (source.FirstOrDefault(x => x.size == ImageSize.large) ??
                            source.FirstOrDefault(x => x.size == ImageSize.medium) ??
                            source.First()).text;

            if (ImageCache.ContainsKey(imageUrl))
                return ImageCache[imageUrl];

            var onlineImage = new OnlineImage(imageUrl);
            ImageCache.Add(imageUrl, onlineImage);
            return onlineImage;
        }

        private static OnlineImage GetSmallImage(IEnumerable<Image> images)
        {
            var imageUrl = images.OrderBy(x => x.size).First().text;
            if (ImageCache.ContainsKey(imageUrl))
                return ImageCache[imageUrl];

            var onlineImage = new OnlineImage(imageUrl);
            ImageCache.Add(imageUrl, onlineImage);
            return onlineImage;
        }
    }

    public class TrackInformation
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string MusicBrainzId { get; set; }
        public ImageProvider CoverImage { get; set; }
    }
}