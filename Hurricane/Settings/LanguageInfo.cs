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

        public LanguageInfo(string code)
        {
            this.Code = code;
        }

        public LanguageInfo(string name, string path, BitmapImage icon, string translator, string code)
        {
            this.Name = name;
            this.Path = path;
            this.Icon = icon;
            this.Translator = translator;
            this.Code = code;
        }

        public LanguageInfo(string name, string path, Uri iconPath, string translator, string code) : this(name, path, new BitmapImage(iconPath), translator, code) { }
    }
}