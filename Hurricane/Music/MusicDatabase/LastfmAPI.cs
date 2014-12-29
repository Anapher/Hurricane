using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Hurricane.Music.MusicDatabase
{
    class LastfmAPI
    {
        public async static Task<BitmapImage> GetImage(string tracktitle, string artist, Settings.ImageQuality imagequality, bool saveimage, DirectoryInfo directory, Track t, bool trimtrackname)
        {
            string apikey = string.Empty;
            apikey = Settings.SensitiveInformations.LastfmAPIKey;

            if (trimtrackname) tracktitle = TrimTrackTitle(tracktitle);

            string url = Uri.EscapeUriString(string.Format("http://ws.audioscrobbler.com/2.0/?method=track.search&track={0}{1}&api_key={2}", Utilities.GeneralHelper.EscapeTitleName(tracktitle), !string.IsNullOrEmpty(artist) ? "&artist=" + Utilities.GeneralHelper.EscapeArtistName(artist) : string.Empty, apikey));
            using (System.Net.WebClient web = new System.Net.WebClient() { Proxy = null })
            {
                string result = await web.DownloadStringTaskAsync(new Uri(url));

                using (StringReader sr = new StringReader(result))
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(lfm));
                    var item = (lfm)xmls.Deserialize(sr);
                    if (item.results.trackmatches.Length > 0)
                    {
                        var foundtrack = item.results.trackmatches[0];
                        url = Uri.EscapeUriString(string.Format("http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={2}&track={0}&artist={1}", foundtrack.name, foundtrack.artist, apikey));
                        result = await web.DownloadStringTaskAsync(url);

                        using (StringReader srtrackinformations = new StringReader(result))
                        {
                            var trackinfo = (lfm)xmls.Deserialize(srtrackinformations);
                            if (trackinfo.track.album != null && trackinfo.track.album.image != null && trackinfo.track.album.image.Length > 0)
                            {
                                string imageurl = GetImageLink(trackinfo.track.album.image, imagequality);

                                if (imageurl != null && !imageurl.EndsWith("default_album_medium.png")) //We don't want the default album art
                                {
                                    BitmapImage img = await DownloadImage(web,imageurl);
                                    string album;
                                    if (string.IsNullOrEmpty(trackinfo.track.album.title))
                                    {
                                        album = string.IsNullOrEmpty(t.Album) ? tracktitle : t.Album;
                                    }
                                    else { album = trackinfo.track.album.title; t.Album = trackinfo.track.album.title; }
                                    if (saveimage) await SaveImage(img, album, directory.FullName);
                                    if (string.IsNullOrEmpty(t.Artist) && trackinfo.track.artist != null && !string.IsNullOrEmpty(trackinfo.track.artist.name))
                                    { t.Artist = trackinfo.track.artist.name; if (!string.IsNullOrEmpty(trackinfo.track.name))  t.Title = trackinfo.track.name; }
                                    
                                    return img;
                                }
                            }

                            if (directory.Exists)
                            {
                                foreach (var file in directory.GetFiles("*.png"))
                                {
                                    if (Utilities.GeneralHelper.EscapeFilename(t.Artist).ToLower() == System.IO.Path.GetFileNameWithoutExtension(file.FullName).ToLower())
                                    {
                                        return new BitmapImage(new Uri(file.FullName));
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
                            else if(!string.IsNullOrEmpty(artist))
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
                                    if (!imageurl.EndsWith("default_album_medium.png")) //We don't want the default album art
                                    {
                                        BitmapImage img = await DownloadImage(web, imageurl);
                                        string artistname;
                                        if (string.IsNullOrEmpty(artistinfo.artist.name))
                                        {
                                            artistname = string.IsNullOrEmpty(t.Artist) ? tracktitle : t.Artist;
                                        }
                                        else { artistname = artistinfo.artist.name; t.Artist = artistinfo.artist.name; }
                                        if (saveimage) await SaveImage(img, artistname, directory.FullName);

                                        return img;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        protected static async Task SaveImage(BitmapImage img, string filename, string directory)
        {
            await Task.Run(() =>
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                string path = System.IO.Path.Combine(directory, Utilities.GeneralHelper.EscapeFilename(filename) + ".png");
                encoder.Frames.Add(BitmapFrame.Create(img));
                using (FileStream filestream = new FileStream(path, FileMode.Create))
                    encoder.Save(filestream);
            });
        }

        protected async static Task<BitmapImage> DownloadImage(WebClient web, string url)
        {
            using (MemoryStream mr = new MemoryStream(await web.DownloadDataTaskAsync(url)))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = mr;
                bitmap.EndInit();
                return bitmap;
            }
        }

        protected static string GetImageLink(lfmArtistImage[] image,Settings.ImageQuality quality)
        {
            if (image.Length == 1) return image[0].Value;
            switch (quality)
            {
                case Settings.ImageQuality.small:
                    return image.First().Value;
                case Settings.ImageQuality.medium:
                case Settings.ImageQuality.large:
                    var items = image.Where((x) => x.size == (quality == Settings.ImageQuality.large ? "large" : "medium"));
                    if (items.Any()) return items.First().Value;
                    break;
            }
            return image.Last().Value;
        }

        protected static string GetImageLink(lfmTrackAlbumImage[] image, Settings.ImageQuality quality)
        {
            if (image.Length == 1) return image[0].Value;
            switch (quality)
            {
                case Settings.ImageQuality.small:
                    return image.First().Value;
                case Settings.ImageQuality.medium:
                case Settings.ImageQuality.large:
                    var items = image.Where((x) => x.size == (quality == Settings.ImageQuality.large ? "large" : "medium"));
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
