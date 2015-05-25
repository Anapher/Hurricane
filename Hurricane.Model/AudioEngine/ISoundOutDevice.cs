namespace Hurricane.Model.AudioEngine
{
    public interface ISoundOutDevice
    {
        string Name { get; }
        string Id { get; }
        bool IsDefault { get; }
    }
}