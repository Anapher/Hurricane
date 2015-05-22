using System;
using System.Net;
using System.Threading.Tasks;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.DataApi
{
    class LastfmApi
    {
        public async static Task<Artist> SearchArtist(string name)
        {
            using (var wc = new WebClient { Proxy = null })
            {
                var searchedArtists =
                    await
                        wc.DownloadStringTaskAsync(
                            $"http://ws.audioscrobbler.com/2.0/?method=artist.search&artist={Uri.EscapeDataString(name)}&api_key={SensitiveInformation.LastfmKey}&format=json&limit=5");
                throw new NotImplementedException();
            }
        }
    }
}
