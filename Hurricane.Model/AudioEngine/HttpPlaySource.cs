using System;

namespace Hurricane.Model.AudioEngine
{
    public class HttpPlaySource : IPlaySource
    {
        public PlaySourceType Type
        {
            get { return PlaySourceType.Http; }
        }

        public Uri WebUri { get; set; }

        public HttpPlaySource(Uri webUri)
        {
            WebUri = webUri;
        }
    }
}