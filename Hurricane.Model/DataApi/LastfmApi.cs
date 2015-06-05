using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetArtistInfo;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.GetTopTracks;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchArtist;
using Hurricane.Model.DataApi.SerializeClasses.Lastfm.SearchTrack;
using Hurricane.Model.Music.Imagment;
using Hurricane.Model.Music.TrackProperties;
using Newtonsoft.Json;
using Artist = Hurricane.Model.Music.TrackProperties.Artist;

namespace Hurricane.Model.DataApi
{
    public class LastfmApi
    {
        public LastfmApi()
        {
            Artists = new List<Artist>();
            ArtistKeywords = new Dictionary<List<string>, Artist>();
        }

        private List<Artist> Artists { get; }
        private Dictionary<List<string>, Artist> ArtistKeywords { get; set; }

        public async Task<Artist> SearchArtist(string name)
        {
            var foundArtist =
                Artists.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (foundArtist != null) return foundArtist;

            using (var wc = new WebClient { Proxy = null })
            {
                var searchedArtists = JsonConvert.DeserializeObject<ArtistSearchResult>(await
                    wc.DownloadStringTaskAsync(
                        $"http://ws.audioscrobbler.com/2.0/?method=artist.search&artist={Uri.EscapeDataString(name)}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"));

                var match = searchedArtists?.results?.ArtistMatches.artist;
                if (match == null) return null;

                var alreadyExistingArtist = Artists.FirstOrDefault(x => x.Name == match.name);
                if (alreadyExistingArtist != null)
                {
                    ArtistKeywords.First(x => x.Value == alreadyExistingArtist).Key.Add(name);
                    return alreadyExistingArtist;
                }

                var artist = new Artist
                {
                    Name = match.name,
                    MusicBrainzId = match.mbid,
                    Url = match.url,
                    Guid = Guid.NewGuid()
                };
                SetImages(artist, match.image);

                Artists.Add(artist);
                ArtistKeywords.Add(new List<string> { name, artist.Name }, artist);
                return artist;
            }
        }

        public async Task<Artist> GetAdvancedInfoAboutArtist(Artist artist, CultureInfo culture)
        {
            if (artist.ProvidesAdvancedInfo)
                return artist;

            using (var wc = new WebClient {Proxy = null})
            {
                var artistInfo = JsonConvert.DeserializeObject<GetArtistInfoResult>(await
                    wc.DownloadStringTaskAsync(string.IsNullOrEmpty(artist.MusicBrainzId)
                        ? $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={Uri.EscapeDataString(artist.Name)}&autocorrect=1&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"
                        : $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={artist.MusicBrainzId}&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"));

                if (string.IsNullOrEmpty(artist.MusicBrainzId))
                    artist.MusicBrainzId = artistInfo.artist.mbid;

                var topTracksTask =
                    wc.DownloadStringTaskAsync(string.IsNullOrEmpty(artist.MusicBrainzId)
                        ? $"http://ws.audioscrobbler.com/2.0/?method=artist.gettoptracks&artist={artist.Name}&api_key={SensitiveInformation.LastfmKey}&format=json"
                        : $"http://ws.audioscrobbler.com/2.0/?method=artist.gettoptracks&mbid={artist.MusicBrainzId}&api_key={SensitiveInformation.LastfmKey}&format=json");

                artist.Biography = artistInfo.artist.bio.content;
                foreach (var similarArtist in artistInfo.artist.similar.artist)
                {
                    var existingArtist = Artists.FirstOrDefault(x => string.Equals(x.Name, similarArtist.name, StringComparison.OrdinalIgnoreCase));
                    if (existingArtist != null)
                    {
                        artist.SimilarArtists.Add(existingArtist);
                        break;
                    }

                    var newArtist = new Artist
                    {
                        Name = similarArtist.name,
                        Url = similarArtist.url,
                        Guid = Guid.NewGuid()
                    };

                    SetImages(newArtist, similarArtist.image);
                    Artists.Add(newArtist);
                    artist.SimilarArtists.Add(newArtist);
                }

                var topTrackResult = JsonConvert.DeserializeObject<GetTopTracksResult>(await topTracksTask);
                artist.TopTracks = topTrackResult.toptracks?.track.Select(x => new TopTrack
                {
                    Artist = artist,
                    Duration = TimeSpan.FromMilliseconds(int.Parse(x.duration)),
                    MusicbrainzId = x.mbid,
                    Name = x.name,
                    Url = x.url,
                    Thumbnail =
                        x.image.Count > 0
                            ? new OnlineImage(
                                (x.image.OrderBy(y => y.size)
                                    .FirstOrDefault(
                                        y =>
                                            y.size != ImageSize.extralarge &&
                                            y.size != ImageSize.mega) ?? x.image.First()).text)
                            : null
                }).ToList();
            }

            artist.ProvidesAdvancedInfo = true;
            return artist;
        }

        public async Task<Artist> GetArtistByMusicbrainzId(string musicbrainzId, CultureInfo culture)
        {
            using (var wc = new WebClient {Proxy = null})
            {
                var artistInfo =
                    JsonConvert.DeserializeObject<GetArtistInfoResult>(
                        await
                            wc.DownloadStringTaskAsync(
                                $"http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={musicbrainzId}&api_key={SensitiveInformation.LastfmKey}&format=json&lang={culture.TwoLetterISOLanguageName}"));
                var artist = new Artist
                {
                    Name = artistInfo.artist.name,
                    MusicBrainzId = artistInfo.artist.mbid,
                    Url = artistInfo.artist.url,
                    Guid = Guid.NewGuid()
                };
                SetImages(artist, artistInfo.artist.image);
                return artist;
            }
        }

        public async Task<TrackInformation> GetTrackInformation(string title, string artist)
        {
            using (var wc = new WebClient {Proxy = null})
            {
                var result =
                    JsonConvert.DeserializeObject<SearchTrackResult>(
                        await
                            wc.DownloadStringTaskAsync(artist == null
                                ? $"http://ws.audioscrobbler.com/2.0/?method=track.search&track={title}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"
                                : $"http://ws.audioscrobbler.com/2.0/?method=track.search&track={title}&artist={artist}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=1"));

                var trackResult = result?.results?.Trackmatches?.track;
                if (trackResult != null)
                {
                    return new TrackInformation
                    {
                        Artist = trackResult.artist,
                        Name = trackResult.name,
                        Url = trackResult.url,
                        MusicBrainzId = trackResult.mbid,
                        CoverImage = trackResult.image?.Count > 0 ? new OnlineImage(trackResult.image.Last().text) : null
                    };
                }
                return null;
            }
        }

        private static void SetImages(Artist artist, List<Image> images)
        {
            if (images == null || images.Count == 0) return;
            images = images.Where(x => !string.IsNullOrEmpty(x.text)).ToList();
            if (images.Count == 0)
                return;
            images = images.OrderBy(x => x.size).ToList();
            artist.SmallImage = new OnlineImage(images.First().text);
            artist.MediumImage =
                new OnlineImage((images.FirstOrDefault(x => x.size == ImageSize.medium) ??
                                   images.FirstOrDefault(x => x.size == ImageSize.large) ??
                                   images.First())?.text);
            artist.LargeImage = new OnlineImage(images.Last().text);
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