using System.Windows.Media;
using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation.AwesomeVisualisation
{
    public class AwesomeAudioVisualisation : AudioVisualisationBase
    {
        private IAudioVisualisationPlugin _loadedPlugin;
        public override IAudioVisualisationPlugin Visualisation
        {
            get { return _loadedPlugin ?? (_loadedPlugin = new AudioVisualisationPlugin()); }
        }

        public override string Name
        {
            get { return "Awesome"; }
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
                            "F1 M 0.000,337.505 L 39.447,337.505 L 39.447,376.954 L 0.000,376.954 L 0.000,337.505 Z",
                            "F1 M 0.000,289.300 L 39.447,289.300 L 39.447,328.733 L 0.000,328.733 L 0.000,289.300 Z",
                            "F1 M 0.000,241.071 L 39.447,241.071 L 39.447,280.512 L 0.000,280.512 L 0.000,241.071 Z",
                            "F1 M 0.000,192.857 L 39.447,192.857 L 39.447,232.306 L 0.000,232.306 L 0.000,192.857 Z",
                            "F1 M 0.000,144.634 L 39.447,144.634 L 39.447,184.081 L 0.000,184.081 L 0.000,144.634 Z",
                            "F1 M 0.000,96.425 L 39.447,96.425 L 39.447,135.868 L 0.000,135.868 L 0.000,96.425 Z",
                            "F1 M 0.000,48.211 L 39.447,48.211 L 39.447,87.658 L 0.000,87.658 L 0.000,48.211 Z",
                            "F1 M 0.000,0.000 L 39.447,0.000 L 39.447,39.447 L 0.000,39.447 L 0.000,0.000 Z",
                            "F1 M 48.218,337.505 L 87.665,337.505 L 87.665,376.954 L 48.218,376.954 L 48.218,337.505 Z",
                            "F1 M 48.218,289.300 L 87.665,289.300 L 87.665,328.733 L 48.218,328.733 L 48.218,289.300 Z",
                            "F1 M 48.218,241.071 L 87.665,241.071 L 87.665,280.512 L 48.218,280.512 L 48.218,241.071 Z",
                            "F1 M 48.218,192.857 L 87.665,192.857 L 87.665,232.306 L 48.218,232.306 L 48.218,192.857 Z",
                            "F1 M 48.218,144.634 L 87.665,144.634 L 87.665,184.081 L 48.218,184.081 L 48.218,144.634 Z",
                            "F1 M 96.435,337.505 L 135.874,337.505 L 135.874,376.954 L 96.435,376.954 L 96.435,337.505 Z",
                            "F1 M 96.435,289.300 L 135.874,289.300 L 135.874,328.733 L 96.435,328.733 L 96.435,289.300 Z",
                            "F1 M 96.435,241.071 L 135.874,241.071 L 135.874,280.512 L 96.435,280.512 L 96.435,241.071 Z",
                            "F1 M 144.645,337.505 L 184.092,337.505 L 184.092,376.954 L 144.645,376.954 L 144.645,337.505 Z",
                            "F1 M 144.645,289.300 L 184.092,289.300 L 184.092,328.733 L 144.645,328.733 L 144.645,289.300 Z",
                            "F1 M 144.645,241.071 L 184.092,241.071 L 184.092,280.512 L 144.645,280.512 L 144.645,241.071 Z",
                            "F1 M 144.645,192.857 L 184.092,192.857 L 184.092,232.306 L 144.645,232.306 L 144.645,192.857 Z",
                            "F1 M 144.645,144.634 L 184.092,144.634 L 184.092,184.081 L 144.645,184.081 L 144.645,144.634 Z",
                            "F1 M 144.645,96.425 L 184.092,96.425 L 184.092,135.868 L 144.645,135.868 L 144.645,96.425 Z",
                            "F1 M 192.859,337.505 L 232.304,337.505 L 232.304,376.954 L 192.859,376.954 L 192.859,337.505 Z",
                            "F1 M 192.859,289.300 L 232.304,289.300 L 232.304,328.733 L 192.859,328.733 L 192.859,289.300 Z",
                            "F1 M 241.085,337.505 L 280.526,337.505 L 280.526,376.954 L 241.085,376.954 L 241.085,337.505 Z",
                            "F1 M 241.085,289.300 L 280.526,289.300 L 280.526,328.733 L 241.085,328.733 L 241.085,289.300 Z",
                            "F1 M 241.085,241.071 L 280.526,241.071 L 280.526,280.512 L 241.085,280.512 L 241.085,241.071 Z",
                            "F1 M 241.085,192.857 L 280.526,192.857 L 280.526,232.306 L 241.085,232.306 L 241.085,192.857 Z",
                            "F1 M 241.085,144.634 L 280.526,144.634 L 280.526,184.081 L 241.085,184.081 L 241.085,144.634 Z",
                            "F1 M 241.085,96.425 L 280.526,96.425 L 280.526,135.868 L 241.085,135.868 L 241.085,96.425 Z",
                            "F1 M 241.085,48.211 L 280.526,48.211 L 280.526,87.658 L 241.085,87.658 L 241.085,48.211 Z",
                            "F1 M 289.299,337.505 L 328.743,337.505 L 328.743,376.954 L 289.299,376.954 L 289.299,337.505 Z",
                            "F1 M 337.504,337.505 L 376.957,337.505 L 376.957,376.954 L 337.504,376.954 L 337.504,337.505 Z",
                            "F1 M 337.504,289.300 L 376.957,289.300 L 376.957,328.733 L 337.504,328.733 L 337.504,289.300 Z",
                            "F1 M 337.504,241.071 L 376.957,241.071 L 376.957,280.512 L 337.504,280.512 L 337.504,241.071 Z",
                            "F1 M 385.725,337.505 L 425.178,337.505 L 425.178,376.954 L 385.725,376.954 L 385.725,337.505 Z",
                            "F1 M 385.725,289.300 L 425.178,289.300 L 425.178,328.733 L 385.725,328.733 L 385.725,289.300 Z",
                            "F1 M 385.725,241.071 L 425.178,241.071 L 425.178,280.512 L 385.725,280.512 L 385.725,241.071 Z",
                            "F1 M 385.725,192.857 L 425.178,192.857 L 425.178,232.306 L 385.725,232.306 L 385.725,192.857 Z"
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