using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hurricane.Model.DataApi.SerializeClasses.Billboard;
using Hurricane.Model.Music.TrackProperties;

namespace Hurricane.Model.DataApi
{
    public class BillboardApi
    {
        public async Task<List<PreviewTrack>>  GetTop100()
        {
            using (var wc = new WebClient { Proxy = null })
            {
                var xmls= new XmlSerializer(typeof(rss));
                using (var textReader = new StringReader(await wc.DownloadStringTaskAsync("https://www.billboard.com/rss/charts/hot-100")))
                {
                    var rssFeed = (rss) xmls.Deserialize(textReader);
                    var result = new List<PreviewTrack>();
                    foreach (var track in rssFeed.channel.item)
                    {
                        result.Add(new PreviewTrack {Name = track.title, Artist = track.artist});

                    }

                    throw new NotImplementedException();
                }
            }
        }
    }
}