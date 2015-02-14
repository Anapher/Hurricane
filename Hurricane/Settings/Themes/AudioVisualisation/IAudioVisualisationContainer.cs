using AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public interface IAudioVisualisationContainer
    {
        IAudioVisualisationPlugin AudioVisualisationPlugin { get; }
        string Name { get; }
    }
}
