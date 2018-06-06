using Newtonsoft.Json;
//list connected devices in a labeld view
namespace Hurricane.AppCommunication
{
    public class ConnectionDeviceInfo
    {
        public string System { get; set; }
        public string Version { get; set; }
        public DeviceType DeviceType { get; set; }
        public string DeviceModel { get; set; }
        public string Name { get; set; }

        public static ConnectionDeviceInfo FromString(string content)
        {
            return JsonConvert.DeserializeObject<ConnectionDeviceInfo>(content);
        }
    }

    public enum DeviceType
    {
        Tablet,
        Smartphone
    }
}