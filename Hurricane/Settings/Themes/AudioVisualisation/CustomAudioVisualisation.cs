using System.IO;
using AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public class CustomAudioVisualisation : IAudioVisualisationContainer
    {
        public string FileName { get; set; }

        private IAudioVisualisationPlugin _loadedPlugin;
        public IAudioVisualisationPlugin AudioVisualisationPlugin
        {
            get { return _loadedPlugin ?? (_loadedPlugin = AudioVisualisationPluginHelper.FromFile(Path.Combine(HurricaneSettings.Instance.AudioVisualisationsDirectory, FileName))); }
        }

        public string Name
        {
            get { return Path.GetFileNameWithoutExtension(FileName); }
        }
    }
}