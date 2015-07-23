using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Hurricane.Model.Services;

namespace Hurricane.Model.Plugins.MusicStreaming
{
    public class MusicStreamingPluginManager
    {
        private IMusicStreamingService _defaultMusicStreaming;

        public MusicStreamingPluginManager()
        {
            MusicStreamingPlugins = new ObservableCollection<MusicStreamingPlugin>();
            LoadedPlugins = new List<PluginInfo>();
        }

        public ObservableCollection<MusicStreamingPlugin> MusicStreamingPlugins { get; }
        public List<PluginInfo> LoadedPlugins { get; }

        public IMusicStreamingService DefaultMusicStreaming
        {
            get { return _defaultMusicStreaming; }
            set
            {
                if (_defaultMusicStreaming != value)
                {
                    /*if (_defaultMusicStreaming != null)
                        _defaultMusicStreaming.IsDefault = false;

                    if (value != null)
                        value.IsDefault = true;*/

                    _defaultMusicStreaming = value;
                }
            }
        }

        public void LoadPlugins(string pluginFolder, IEnumerable<IMusicStreamingService> defaultServices)
        {
           /* var directoryInfo = new DirectoryInfo(pluginFolder);
            if (!directoryInfo.Exists)
                directoryInfo.Create();*/

            DefaultMusicStreaming = defaultServices.First();
        }
    }
}