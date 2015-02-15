using System;
using System.Windows;
using System.Windows.Media.Imaging;
using AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation.DefaultAudioVisualisation
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

        private DefaultAudioVisualisation()
        {
        }

        private static DefaultAudioVisualisation _instance;
        public static DefaultAudioVisualisation GetDefault()
        {
            return _instance ?? (_instance = new DefaultAudioVisualisation());
        }

        public class AudioVisualisation : IAudioVisualisationPlugin
        {
            private IAudioVisualisation _advancedAudioVisualisation;
            public IAudioVisualisation AdvancedWindowVisualisation
            {
                get
                {
                    return _advancedAudioVisualisation ??
                           (_advancedAudioVisualisation = new AdvancedWindowAudioVisualisation());
                }
            }

            private IAudioVisualisation _smartWindowAudioVisualisation;
            public IAudioVisualisation SmartWindowVisualisation
            {
                get
                {
                    return _smartWindowAudioVisualisation ??
                           (_smartWindowAudioVisualisation = new SmartWindowAudioVisualisation());
                }
            }

            public string Creator
            {
                get { return "Akaline"; }
            }

            private BitmapImage _thumbnail;
            public BitmapImage Thumbnail
            {
                get { return _thumbnail ?? (_thumbnail = new BitmapImage(new Uri("/Resources/App/AudioVisualisation.png", UriKind.Relative))); }
            }

            public void Refresh()
            {
                if (_advancedAudioVisualisation != null) _advancedAudioVisualisation.Dispose();
                if (_smartWindowAudioVisualisation != null) _smartWindowAudioVisualisation.Disable();

                _advancedAudioVisualisation = null;
                _smartWindowAudioVisualisation = null;
            }
        }
    }
}