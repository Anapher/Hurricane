using System.Windows;
using System.Windows.Media;
using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation.BarAudioVisualisation
{
    public class BarAudioVisualisation : AudioVisualisationBase
    {
        private IAudioVisualisationPlugin _loadedPlugin;
        public override IAudioVisualisationPlugin Visualisation
        {
            get { return _loadedPlugin ?? (_loadedPlugin = new AudioVisualisationPlugin()); }
        }

        public override string Name
        {
            get { return Application.Current.Resources["Bars"].ToString(); }
        }

        public class AudioVisualisationPlugin : IAudioVisualisationPlugin
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

            private GeometryGroup _thumbnail;
            public GeometryGroup Thumbnail
            {
                get
                {
                    if (_thumbnail == null)
                    {
                        var values = new[]
                        {
                            "F1 M 0.000,47.385 L 4.167,47.385 L 4.167,56.696 L 0.000,56.696 L 0.000,47.385 Z",
                            "F1 M 0.000,41.991 L 4.167,41.991 L 4.167,45.236 L 0.000,45.236 L 0.000,41.991 Z",
                            "F1 M 6.250,38.866 L 10.417,38.866 L 10.417,42.111 L 6.250,42.111 L 6.250,38.866 Z",
                            "F1 M 12.500,34.700 L 16.667,34.700 L 16.667,37.945 L 12.500,37.945 L 12.500,34.700 Z",
                            "F1 M 18.750,36.783 L 22.917,36.783 L 22.917,40.028 L 18.750,40.028 L 18.750,36.783 Z",
                            "F1 M 25.000,25.325 L 29.167,25.325 L 29.167,28.570 L 25.000,28.570 L 25.000,25.325 Z",
                            "F1 M 31.251,0.000 L 35.417,0.000 L 35.417,3.245 L 31.251,3.245 L 31.251,0.000 Z",
                            "F1 M 37.501,4.167 L 41.667,4.167 L 41.667,7.412 L 37.501,7.412 L 37.501,4.167 Z",
                            "F1 M 43.751,10.417 L 47.917,10.417 L 47.917,13.662 L 43.751,13.662 L 43.751,10.417 Z",
                            "F1 M 50.001,25.000 L 54.167,25.000 L 54.167,28.244 L 50.001,28.244 L 50.001,25.000 Z",
                            "F1 M 56.251,21.875 L 60.417,21.875 L 60.417,25.120 L 56.251,25.120 L 56.251,21.875 Z",
                            "F1 M 6.250,44.281 L 10.417,44.281 L 10.417,56.695 L 6.250,56.695 L 6.250,44.281 Z",
                            "F1 M 12.500,40.289 L 16.667,40.289 L 16.667,56.695 L 12.500,56.695 L 12.500,40.289 Z",
                            "F1 M 25.000,30.653 L 29.167,30.653 L 29.167,56.695 L 25.000,56.695 L 25.000,30.653 Z",
                            "F1 M 31.251,5.718 L 35.417,5.718 L 35.417,56.696 L 31.251,56.696 L 31.251,5.718 Z",
                            "F1 M 37.501,9.885 L 41.667,9.885 L 41.667,56.696 L 37.501,56.696 L 37.501,9.885 Z",
                            "F1 M 43.751,16.135 L 47.917,16.135 L 47.917,56.696 L 43.751,56.696 L 43.751,16.135 Z",
                            "F1 M 50.001,30.653 L 54.167,30.653 L 54.167,56.695 L 50.001,56.695 L 50.001,30.653 Z",
                            "F1 M 56.251,27.268 L 60.417,27.268 L 60.417,56.696 L 56.251,56.696 L 56.251,27.268 Z",
                            "F1 M 18.750,42.198 L 22.917,42.198 L 22.917,56.695 L 18.750,56.695 L 18.750,42.198 Z"
                        };
                        _thumbnail = new GeometryGroup();
                        foreach (var value in values)
                        {
                            _thumbnail.Children.Add(Geometry.Parse(value));
                        }
                        _thumbnail.Freeze();
                    }
                    return _thumbnail;
                }
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