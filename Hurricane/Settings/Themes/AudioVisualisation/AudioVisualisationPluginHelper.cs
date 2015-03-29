using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Hurricane.PluginAPI.AudioVisualisation;

namespace Hurricane.Settings.Themes.AudioVisualisation
{
    public class AudioVisualisationPluginHelper
    {
        public static IAudioVisualisationPlugin FromFile(string fileName)
        {
            var assembly = AssemblyName.GetAssemblyName(fileName);
            var library = Assembly.Load(assembly);
            foreach (var type in library.GetTypes().Where(type => type.GetInterface("IAudioVisualisationPlugin") != null))
            {
                return Activator.CreateInstance(type) as IAudioVisualisationPlugin;
            }
            throw new Exception();
        }

        public static IAudioVisualisationPlugin FromStream(Stream streamSource)
        {
            using (var memoryStream = new MemoryStream())
            {
                streamSource.CopyTo(memoryStream);
                var library = Assembly.Load(memoryStream.ToArray());
                foreach (var type in library.GetTypes().Where(type => type.GetInterface("IAudioVisualisationPlugin") != null))
                {
                    return Activator.CreateInstance(type) as IAudioVisualisationPlugin;
                }
            }
            throw new Exception();
        }
    }
}