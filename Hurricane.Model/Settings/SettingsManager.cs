using System.IO;
using System.Xml.Serialization;

namespace Hurricane.Model.Settings
{
    public static class SettingsManager
    {
        public static SettingsData Current { get; set; }

        public static void Load(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var serializer = new XmlSerializer(typeof(SettingsData));
                Current = (SettingsData) serializer.Deserialize(fileStream);
            }
        }

        public static void InitalizeNew()
        {
            Current = new SettingsData();
            Current.Initalize();
        }

        public static void Save(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var serializer = new XmlSerializer(typeof(SettingsData));
                    serializer.Serialize(fs, Current);
                }
                File.Copy(tempFile, path, true);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}