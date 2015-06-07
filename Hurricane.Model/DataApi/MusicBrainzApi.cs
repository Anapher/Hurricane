using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hurricane.Model.DataApi.SerializeClasses.MusicBrainz.GetArtistByTrackId;
using Newtonsoft.Json;

namespace Hurricane.Model.DataApi
{
    public class MusicBrainzApi
    {
        public static async Task<string> GetArtistIdByTrackId(string trackId)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    var result =
                        JsonConvert.DeserializeObject<GetArtistByTrackIdResult>(
                            await
                                wc.DownloadStringTaskAsync(
                                    $"https://musicbrainz.org/ws/2/recording/{trackId}?inc=artist-credits&fmt=json"));
                    return result?.artistCredits?.FirstOrDefault()?.artist.id;
                }
            }
            catch (WebException) //Sometimes, there comes a 502 gateway error
            {
                return null;
            }
        }
    }
}