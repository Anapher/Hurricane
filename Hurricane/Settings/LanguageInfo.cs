using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Hurricane.Settings
{
    [Serializable]
    public class LanguageInfo
    {
        [XmlIgnore]
        public string Name { get; set; }

        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public BitmapImage Icon { get; set; }

        [XmlIgnore]
        public string Translator { get; set; }

        public string Code { get; set; }

        public void Load(IEnumerable<LanguageInfo> list)
        {
            foreach (LanguageInfo info in list)
            {
                if (info.Code == this.Code)
                {
                    this.Name = info.Name;
                    this.Path = info.Path;
                    this.Icon = info.Icon;
                    this.Translator = info.Translator;
                    return;
                }
            }
            throw new ArgumentException(string.Format("The current code {0} isn't in the list", this.Code));
        }

        public LanguageInfo()
        {
        }

        public LanguageInfo(string Code)
        {
            this.Code = Code;
        }

        public LanguageInfo(string Name, string Path, BitmapImage Icon, string Translator, string Code)
        {
            this.Name = Name;
            this.Path = Path;
            this.Icon = Icon;
            this.Translator = Translator;
            this.Code = Code;
        }

        public LanguageInfo(string Name, string Path, Uri IconPath, string Translator, string Code) : this(Name, Path, new BitmapImage(IconPath), Translator, Code) { }
    }
}