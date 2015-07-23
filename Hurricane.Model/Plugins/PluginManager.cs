using System;
using System.IO;
using Hurricane.Model.Plugins.MusicStreaming;

namespace Hurricane.Model.Plugins
{
    public class PluginManager
    {
        private bool _isLoaded;
        private DirectoryInfo _pluginDirectory;

        internal PluginManager()
        {
            
        }

        public MusicStreamingPluginManager MusicStreamingPluginManager { get; set; }

        public void Load(string pluginFolder)
        {
            if (_isLoaded)
                throw new InvalidOperationException();

            _pluginDirectory = new DirectoryInfo(pluginFolder);
            if (!_pluginDirectory.Exists)
                _pluginDirectory.Create();

            var pluginDataFile = new FileInfo(Path.Combine(_pluginDirectory.FullName, "PluginInfo.json"));
            MusicStreamingPluginManager = new MusicStreamingPluginManager();

        }
    }
}