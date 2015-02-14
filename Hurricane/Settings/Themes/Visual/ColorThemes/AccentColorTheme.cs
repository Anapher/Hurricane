using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

namespace Hurricane.Settings.Themes.Visual.ColorThemes
{
    [Serializable]
    public class AccentColorTheme : ColorThemeBase
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

        public override void ApplyTheme()
        {
            var resource = new ResourceDictionary { Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Name), UriKind.Relative) };
            ApplicationThemeManager.Instance.LoadResource("colortheme", resource);
        }
    }
}
