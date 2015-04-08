using System;
using System.Collections.Generic;
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
                if (info.Code == Code)
                {
                    Name = info.Name;
                    Path = info.Path;
                    Icon = info.Icon;
                    Translator = info.Translator;
                    return;
                }
            }
            throw new ArgumentException(string.Format("The current code {0} isn't in the list", Code));
        }

        public LanguageInfo()
        {
        }

        public LanguageInfo(string code)
        {
            Code = code;
        }

        public LanguageInfo(string name, string path, BitmapImage icon, string translator, string code)
        {
            Name = name;
            Path = path;
            Icon = icon;
            Translator = translator;
            Code = code;
        }

        public LanguageInfo(string name, string path, Uri iconPath, string translator, string code) : this(name, path, new BitmapImage(iconPath), translator, code) { }
    }
}