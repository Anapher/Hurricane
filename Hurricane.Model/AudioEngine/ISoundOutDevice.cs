namespace Hurricane.Model.AudioEngine
{
    public interface ISoundOutDevice
    {
        string Name { get; }
        string Id { get; }
        string SoundOutMode { get; }
        bool IsDefault { get; set; }
    }
}