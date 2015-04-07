using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hurricane.Music.Proxy
{
    public class ProxyManager
    {
        private static ProxyManager _instance;
        public static ProxyManager Instance
        {
            get { return _instance ?? (_instance = new ProxyManager()); }
        }


        private ProxyManager()
        {
            InvalidHttpProxies = new List<string>();
        }

        public List<string> InvalidHttpProxies { get; set; }

        public async Task<HttpProxy> GetWebProxy()
        {
            var proxies = await GetProxies();
            var sortedList =
              proxies.OrderByDescending(x => x.Country == "United Kingdom").ThenBy(x => x.Speed).ThenBy(x => x.ResponseTime);

            var proxy = sortedList.FirstOrDefault(x => !InvalidHttpProxies.Contains(x.DecodeIp() + ":" + x.Port));
            if (proxy == null)
                throw new Exception("No proxies found");

            return new HttpProxy(proxy.DecodeIp(), proxy.Port);
        }

        public void AddInvalid(HttpProxy proxy)
        {
            InvalidHttpProxies.Add(proxy.ToString());
        }

        private async Task<List<ProxyEntry>> GetProxies()
        {
            var result = new List<ProxyEntry>();

            using (var wc = new WebClient { Proxy = null })
            {
                var content = await wc.DownloadStringTaskAsync("http://www.cool-proxy.net/proxies/http_proxy_list/sort:score/direction:desc");
                var regex = new Regex(@"document.write\(Base64.decode\(str_rot13\(""(?<ip>(.*?))""\)\)\)<\/script><\/td>\s+<td>(?<port>([0-9]+))<\/td>.*?<td>.*?<td>(?<country>(.*?))<\/td>.*?;"">(?<working>([0-9]+))<\/span><\/div><\/td>.*?#.{6};"">(?<responsetime>(.*?))<\/span><\/div><\/td>.*?>(?<speed>([0-9]+))<\/span><\/div><\/td>");
                result.AddRange(from Match match in regex.Matches(content.Replace("\n", null).Replace("\r", null))
                    select new ProxyEntry
                    {
                        Country = match.Groups["country"].Value,
                        Ip = match.Groups["ip"].Value,
                        Port = int.Parse(match.Groups["port"].Value),
                        Speed = int.Parse(match.Groups["speed"].Value),
                        ResponseTime = double.Parse(match.Groups["responsetime"].Value, CultureInfo.InvariantCulture),
                        Working = int.Parse(match.Groups["working"].Value)
                    });
            }
            return result;
        }
    }
}
