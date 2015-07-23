using System.Windows;

namespace Hurricane.Model.Plugins
{
    public static class PluginExtension
    {
        private static readonly PluginManager CurrentPluginManager;

        static PluginExtension()
        {
            CurrentPluginManager = new PluginManager();
        }

        public static PluginManager PluginManager(this Application application)
        {
            return CurrentPluginManager;
        }
    }
}