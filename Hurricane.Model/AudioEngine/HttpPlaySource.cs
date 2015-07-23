using System;

namespace Hurricane.Model.AudioEngine
{
    public class HttpPlaySource : IPlaySource
    {
        public PlaySourceType Type => PlaySourceType.Http;
        public int Bitrate { get; set; }

        public Uri WebUri { get; }

        public HttpPlaySource(Uri webUri)
        {
            WebUri = webUri;
        }
    }
}