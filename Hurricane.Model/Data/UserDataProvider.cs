using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hurricane.Model.Data
{
    public class UserDataProvider
    {
        public UserDataProvider()
        {
            UserData = new UserData();
        }

        public UserData UserData { get; set; }

        public async Task LoadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var serializer = new XmlSerializer(typeof(UserData));
                // ReSharper disable once AccessToDisposedClosure
                UserData = await Task.Run(() => (UserData)serializer.Deserialize(fs));
            }
        }

        public void SaveToFile(string path)
        {
            var tempFile = Path.GetTempFileName(); //We serialize to a temp file
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var serializer = new XmlSerializer(typeof(UserData));
                    serializer.Serialize(fs, UserData);
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