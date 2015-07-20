using Hurricane.Model.Services;

namespace Hurricane.Model.Plugins.MusicStreaming
{
    public class MusicStreamingPlugin
    {
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }

        public IMusicStreamingService MusicStreamingService { get; set; }
    }
}