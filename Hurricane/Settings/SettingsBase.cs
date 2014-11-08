using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hurricane.Settings
{
    [Serializable]
    public abstract class SettingsBase : ViewModelBase.PropertyChangedBase
    {
        public abstract void SetStandardValues();

        public abstract void Save(string ProgramPath); 

        protected void Save<T>(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, this);
            }
        }
    }
}
