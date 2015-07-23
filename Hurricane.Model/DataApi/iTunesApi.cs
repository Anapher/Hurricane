using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Model.DataApi.SerializeClasses.iTunes;
using Hurricane.Model.Music.TrackProperties;
using Newtonsoft.Json;

namespace Hurricane.Model.DataApi
{
    // ReSharper disable once InconsistentNaming
    public class iTunesApi
    {
        public async static Task<List<PreviewTrack>> GetTop100(CultureInfo culture)
        {
            using (var wc = new WebClient {Proxy = null, Encoding = Encoding.UTF8})
            {
                var data = JsonConvert.DeserializeObject<RssFeed>(await wc.DownloadStringTaskAsync($"https://itunes.apple.com/{culture.TwoLetterISOLanguageName}/rss/topsongs/limit=100/json"));
                int counter = 1;
                return
                    data.feed.entry.Select(
                        x =>
                            new PreviewTrack
                            {
                                Artist = x.Artist.label,
                                Name = x.Name.label,
                                ImageUrl = x.Image[2].label,
                                Number = counter++
                            }).ToList();
            }
        }
    }
}