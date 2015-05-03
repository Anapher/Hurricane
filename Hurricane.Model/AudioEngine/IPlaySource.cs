namespace Hurricane.Model.AudioEngine
{
    /// <summary>
    /// Marks the object as playable for an <see cref="Hurricane.Model.AudioEngine.IAudioEngine"/>
    /// </summary>
    public interface IPlaySource
    {
        /// <summary>
        /// How the <see cref="Hurricane.Model.AudioEngine.IAudioEngine"/> should open the audio
        /// </summary>
        PlaySourceType Type { get; }
    }

    /// <summary>
    /// The type of a <see cref="Hurricane.Model.AudioEngine.IPlaySource"/>
    /// </summary>
    public enum PlaySourceType
    {
        /// <summary>
        /// The <see cref="Hurricane.Model.AudioEngine.IPlaySource"/> is a local file
        /// </summary>
        LocalFile,
        /// <summary>
        /// The <see cref="Hurricane.Model.AudioEngine.IPlaySource"/> is an url to a http address
        /// </summary>
        Http,
        /// <summary>
        /// The <see cref="Hurricane.Model.AudioEngine.IPlaySource"/> is a <see cref="System.IO.Stream"/>
        /// </summary>
        Stream
    }
}