using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public interface IAudioVisualisationContainer
    {
        IAudioVisualisationPlugin Visualisation { get; }
        string Name { get; }
    }
}
