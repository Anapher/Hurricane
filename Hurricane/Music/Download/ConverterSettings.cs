using System.Diagnostics.CodeAnalysis;

namespace Hurricane.Music.Download
{
    public class ConverterSettings
    {
        public bool IsEnabled { get; set; }
        public int Quality { get; set; }
        public AudioFormat Format { get; set; }

        public void SetDefault()
        {
            IsEnabled = false;
            Quality = 10;
            Format = AudioFormat.Copy;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AudioFormat
    {
        Copy,
        MP3,
        AAC,
        WMA
    }
}
