using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Exceptionless.Json;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Hurricane.Utilities;

namespace Hurricane.Music.MusicCover.APIs.Lastfm
{
    class LastfmAPI
    {
        public async static Task<BitmapImage> GetImage(ImageQuality imagequality, bool saveimage, DirectoryInfo directory, PlayableBase track, bool trimtrackname, bool useArtist = true)
        {
            return null;
            string apikey = SensitiveInformation.LastfmKey;

            string title = track.Title;
            string artist = useArtist ? track.Artist : string.Empty;
            if (trimtrackname) title = TrimTrackTitle(track.Title);

            string url = Uri.EscapeUriString(string.Format("http://ws.audioscrobbler.com/2.0/?method=track.search&track={0}{1}&api_key={2}&format=json", title.ToEscapedUrl(), !string.IsNullOrEmpty(artist) ? "&artist=" + artist.ToEscapedUrl() : string.Empty, apikey));
            using (WebClient web = new WebClient { Proxy = null })
            {
                string result = await web.DownloadStringTaskAsync(new Uri(url));
                var item = JsonConvert.DeserializeObject<LfmSearchResult>(result);
                if (item.results.trackmatches.track != null && item.results.trackmatches.track.Count > 0)
                {
                    var foundtrack = item.results.trackmatches.track[0];
                    url = Uri.EscapeUriString(string.Format("http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={2}&track={0}&artist={1}", foundtrack.name, foundtrack.artist, apikey));
                    result = await web.DownloadStringTaskAsync(url);
                    /*
                    using (var srtrackinformation = new StringReader(result))
                    {
                        var trackinfo = (lfm)xmls.Deserialize(srtrackinformation);
                        if (trackinfo.track.album != null && trackinfo.track.album.image != null && trackinfo.track.album.image.Length > 0)
                        {
                            string imageurl = GetImageLink(trackinfo.track.album.image, imagequality);

                            if (imageurl != null && !imageurl.EndsWith("default_album_medium.png") && !imageurl.EndsWith("[unknown].png")) //We don't want the default album art
                            {
                                BitmapImage img = await ImageHelper.DownloadImage(web, imageurl);
                                string album;
                                if (string.IsNullOrEmpty(trackinfo.track.album.title))
                                {
                                    album = string.IsNullOrEmpty(track.Album) ? title : track.Album;
                                }
                                else { album = trackinfo.track.album.title; }
                                if (saveimage) await ImageHelper.SaveImage(img, album, directory.FullName);

                                return img;
                            }
                        }

                        if (directory.Exists)
                        {
                            foreach (var file in directory.GetFiles("*.png"))
                            {
                                if (artist.ToEscapedFilename().ToLower() == Path.GetFileNameWithoutExtension(file.FullName).ToLower())
                                {
                                    var img = new BitmapImage(new Uri(file.FullName));
                                    img.Freeze();
                                    return img;
                                }
                            }
                        }

                        if (trackinfo.track.artist != null && !string.IsNullOrEmpty(trackinfo.track.artist.mbid))
                        {
                            url = string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&mbid={0}&api_key={1}", trackinfo.track.artist.mbid, apikey);
                        }
                        else if (trackinfo.track.artist != null && !string.IsNullOrEmpty(trackinfo.track.artist.name))
                        {
                            url = string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={0}&api_key={1}", trackinfo.track.artist.name, apikey);
                        }
                        else if (!string.IsNullOrEmpty(artist))
                        {
                            url = string.Format("http://ws.audioscrobbler.com/2.0/?method=artist.getinfo&artist={0}&autocorrect=1&api_key={1}", artist, apikey);
                        }

                        if (string.IsNullOrEmpty(url)) return null;
                        result = await web.DownloadStringTaskAsync(Uri.EscapeUriString(url));
                        using (StringReader srartist = new StringReader(result))
                        {
                            lfm artistinfo = (lfm)xmls.Deserialize(srartist);
                            if (artistinfo.artist != null && artistinfo.artist.image != null && artistinfo.artist.image.Length > 0)
                            {
                                string imageurl = GetImageLink(artistinfo.artist.image, imagequality);
                                if (imageurl == null) return null;
                                if (!imageurl.EndsWith("default_album_medium.png") && !imageurl.EndsWith("[unknown].png")) //We don't want the default album art
                                {
                                    BitmapImage img = await ImageHelper.DownloadImage(web, imageurl);
                                    string artistname;
                                    if (string.IsNullOrEmpty(artistinfo.artist.name))
                                    {
                                        artistname = string.IsNullOrEmpty(artist) ? track.Title : artist;
                                    }
                                    else { artistname = artistinfo.artist.name; }
                                    if (saveimage) await ImageHelper.SaveImage(img, artistname, directory.FullName);

                                    return img;
                                }
                            }
                            
                        }
                }*/
                }
            }

            return null;
        }

        protected static string GetImageLink(lfmArtistImage[] image, ImageQuality quality)
        {
            if (image.Length == 1) return image[0].Value;
            switch (quality)
            {
                case ImageQuality.Small:
                    return image.First().Value;
                case ImageQuality.Medium:
                case ImageQuality.Large:
                    var items = image.Where((x) => x.size == (quality == ImageQuality.Large ? "large" : "medium"));
                    if (items.Any()) return items.First().Value;
                    break;
            }
            return image.Last().Value;
        }

        protected static string GetImageLink(lfmTrackAlbumImage[] image, ImageQuality quality)
        {
            if (image.Length == 1) return image[0].Value;
            switch (quality)
            {
                case ImageQuality.Small:
                    return image.First().Value;
                case ImageQuality.Medium:
                case ImageQuality.Large:
                    var items = image.Where((x) => x.size == (quality == ImageQuality.Large ? "large" : "medium"));
                    if (items.Any()) return items.First().Value;
                    break;
            }
            return image.Last().Value;
        }

        protected static string TrimTrackTitle(string title)
        {
            title = title.ToLower();
            title = title.Replace("official music", string.Empty);

            title = title.Trim(new char[] { ' ', '-' });
            if (title.EndsWith("hd")) title = title.Remove(title.Length - 2, 2);
            return Regex.Replace(title, @"[\[\(](?!ft).*[\]\)]", "");
        }

        //David Guetta - Memories (ft. Kid Cudi) [with lyrics]
        //Memories - David guetta feat Kid cudi - Official Music
        //David Guetta ft. Kid Cudi Memories (Not Official) HD
    }
}