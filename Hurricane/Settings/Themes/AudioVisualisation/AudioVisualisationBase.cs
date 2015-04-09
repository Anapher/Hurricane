using System.Xml.Serialization;
using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    [XmlInclude(typeof(BarAudioVisualisation.BarAudioVisualisation)),
    XmlInclude(typeof(SquareAudioVisualisation.SquareAudioVisualisation))]
    public abstract class AudioVisualisationBase : IAudioVisualisationContainer
    {
        public abstract IAudioVisualisationPlugin Visualisation { get; }
        public abstract string Name { get; }
    }
}