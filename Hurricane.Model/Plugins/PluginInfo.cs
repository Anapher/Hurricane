using Newtonsoft.Json;

namespace Hurricane.Model.Plugins
{
    public class PluginInfo
    {
        public string Path { get; set; }
        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public object Plugin { get; set; }
    }
}