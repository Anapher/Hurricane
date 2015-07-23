using System.IO;

namespace Hurricane.Model.AudioEngine
{
    public class StreamPlaySource : IPlaySource
    {
        public PlaySourceType Type => PlaySourceType.Stream;

        public Stream Stream { get; }
        public int Bitrate { get; set; }

        public StreamPlaySource(Stream stream)
        {
            Stream = stream;
        }
    }
}