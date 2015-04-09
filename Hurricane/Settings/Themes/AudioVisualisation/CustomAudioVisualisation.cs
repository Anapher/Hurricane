using System.IO;
using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public class CustomAudioVisualisation : IAudioVisualisationContainer
    {
        public string FileName { get; set; }

        private IAudioVisualisationPlugin _loadedPlugin;
        public IAudioVisualisationPlugin Visualisation
        {
            get { return _loadedPlugin ?? (_loadedPlugin = AudioVisualisationPluginHelper.FromFile(Path.Combine(HurricaneSettings.Paths.AudioVisualisationsDirectory, FileName))); }
        }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(FileName); }
        }
    }
}