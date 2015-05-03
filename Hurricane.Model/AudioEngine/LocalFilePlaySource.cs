namespace Hurricane.Model.AudioEngine
{
    class LocalFilePlaySource : IPlaySource
    {
        public PlaySourceType Type
        {
            get { return PlaySourceType.LocalFile; }
        }

        public string Path { get; private set; }

        public LocalFilePlaySource(string path)
        {
            Path = path;
        }
    }
}
