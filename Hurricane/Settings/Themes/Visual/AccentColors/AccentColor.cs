using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

namespace Hurricane.Settings.Themes.Visual.AccentColors
{
    [Serializable]
    public class AccentColor : AccentColorBase
    {
        [XmlIgnore]
        public Brush BorderColorBrush { get; set; }

        [XmlIgnore]
        public override Brush ColorBrush
        {
            get
            {
                return ThemeManager.Accents.First(x => x.Name == Name).Resources["AccentColorBrush"] as Brush;
            }
        }

        [XmlIgnore]
        public override string TranslatedName
        {
            get { return Application.Current.Resources[Name].ToString(); }
        }

        public override string Group
        {
            get { return Application.Current.Resources["Default"].ToString(); }
        }

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary
        {
            get { return new ResourceDictionary { Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Name), UriKind.Relative) }; }
        }
    }
}
