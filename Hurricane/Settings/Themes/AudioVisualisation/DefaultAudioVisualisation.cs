using System.Windows;
using AudioVisualisation;
using WPFSoundVisualizationLib;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public class DefaultAudioVisualisation : IAudioVisualisationContainer
    {
        private IAudioVisualisationPlugin _loadedPlugin;
        public IAudioVisualisationPlugin AudioVisualisationPlugin
        {
            get { return _loadedPlugin ?? (_loadedPlugin = new AudioVisualisation()); }
        }

        public string Name
        {
            get { return Application.Current.Resources["Default"].ToString(); }
        }

        public class AudioVisualisation : IAudioVisualisationPlugin
        {
            public UIElement GetUIElement(ISpectrumPlayer spectrumPlayer)
            {
                var spectrumAnalyzer = new SpectrumAnalyzer();
                spectrumAnalyzer.RegisterSoundPlayer(spectrumPlayer);
                return spectrumAnalyzer;
            }

            public string Creator
            {
                get { return "Akaline"; }
            }
        }

        private DefaultAudioVisualisation()
        {
        }

        private static DefaultAudioVisualisation _instance;
        public static DefaultAudioVisualisation GetDefault()
        {
            return _instance ?? (_instance = new DefaultAudioVisualisation());
        }
    }
}