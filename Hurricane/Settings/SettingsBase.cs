using System;
using System.IO;
using System.Xml.Serialization;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings
{
    [Serializable]
    public abstract class SettingsBase : PropertyChangedBase
    {
        public abstract void SetStandardValues();

        public abstract void Save(string programPath);

        protected void Save<T>(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, this);
            }
        }
    }
}
