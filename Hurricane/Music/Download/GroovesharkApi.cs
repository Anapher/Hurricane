using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music.Download
{
    class GroovesharkApi
    {
        public class HttpHeaders
        {
            [JsonProperty("Accept-Charset")]
            public string AcceptCharset { get; set; }

            [JsonProperty("Content-Length")]
            public int ContentLength { get; set; }

            [JsonProperty("Accept-Language")]
            public string AcceptLanguage { get; set; }

            [JsonProperty("Accept-Encoding")]
            public string AcceptEncoding { get; set; }

            public string Cookie { get; set; }

            [JsonProperty("Content-Type")]
            public string ContentType { get; set; }

            public string Accept { get; set; }

            [JsonProperty("User-Agent")]
            public string UserAgent { get; set; }
        }

        public class GroovesharkResult
        {
            [JsonProperty("display_id")]
            public string DisplayId { get; set; }

            [JsonProperty("extractor")]
            public string Extractor { get; set; }

            [JsonProperty("http_post_data")]
            public string HttpPostData { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }

            [JsonProperty("requested_subtitles")]
            public object RequestedSubtitles { get; set; }

            [JsonProperty("http_method")]
            public string HttpMethod { get; set; }

            [JsonProperty("duration")]
            public int Duration { get; set; }

            [JsonProperty("format_id")]
            public string FormatId { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("playlist")]
            public object Playlist { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("extractor_key")]
            public string ExtractorKey { get; set; }

            [JsonProperty("http_headers")]
            public HttpHeaders HttpHeaders { get; set; }

            [JsonProperty("playlist_index")]
            public object PlaylistIndex { get; set; }

            [JsonProperty("ext")]
            public string Extension { get; set; }

            [JsonProperty("webpage_url")]
            public string WebpageUrl { get; set; }

            [JsonProperty("_filename")]
            public string Filename { get; set; }

            [JsonProperty("fulltitle")]
            public string FullTitle { get; set; }

            [JsonProperty("webpage_url_basename")]
            public string WebpageUrlBasename { get; set; }
        }

        private static bool? _isProxyRequired;
        public async static Task<bool> IsProxyRequired()
        {
            if (_isProxyRequired.HasValue) return _isProxyRequired.Value;
            using (var wc = new WebClient {Proxy = null})
            {
                _isProxyRequired = (await wc.DownloadStringTaskAsync("http://grooveshark.com/")).Contains("heartbroken");
                return _isProxyRequired.Value;
            }
        }
    }
}