using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hurricane.Model.Services;

namespace Hurricane.Model.Plugins.MusicStreaming
{
    public class MusicStreamingPluginManager
    {
        private MusicStreamingPlugin _defaultMusicStreaming;

        public MusicStreamingPluginManager()
        {
            MusicStreamingPlugins = new ObservableCollection<MusicStreamingPlugin>();
        }

        public ObservableCollection<MusicStreamingPlugin> MusicStreamingPlugins { get; }

        public MusicStreamingPlugin DefaultMusicStreaming
        {
            get { return _defaultMusicStreaming; }
            set
            {
                if (_defaultMusicStreaming != value)
                {
                    if (_defaultMusicStreaming != null)
                        _defaultMusicStreaming.IsDefault = false;

                    if (value != null)
                        value.IsDefault = true;

                    _defaultMusicStreaming = value;
                }
            }
        }

        public void LoadPlugins(string pluginFolder, IEnumerable<IMusicStreamingService> defaultServices)
        {
            foreach (var musicStreamingService in defaultServices)
            {
                MusicStreamingPlugins.Add(new MusicStreamingPlugin
                {
                    IsEnabled = true,
                    MusicStreamingService = musicStreamingService
                });
            }

            DefaultMusicStreaming = MusicStreamingPlugins.First();
        }
    }
}