using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes.Visual.ColorThemes
{
    [Serializable]
    public class CustomColorTheme : ColorThemeBase
    {
        public static bool FromFile(string filename, out CustomColorTheme result)
        {
            var colorTheme = new CustomColorTheme { Name = new FileInfo(filename).Name };

            try
            {
                colorTheme.GetResource();
            }
            catch (XamlParseException)
            {
                result = null;
                return false;
            }

            result = colorTheme;
            return true;
        }

        [XmlIgnore]
        public override string TranslatedName
        {
            get { return Path.GetFileNameWithoutExtension(Name); }
        }

        private Brush _colorBrush;
        [XmlIgnore]
        public override Brush ColorBrush
        {
            get { return _colorBrush ?? (_colorBrush = GetResource()["AccentColorBrush"] as Brush); }
        }

        private ResourceDictionary GetResource()
        {
            var foo = new Uri(Path.Combine(HurricaneSettings.Instance.ColorThemesDirectory, Name), UriKind.RelativeOrAbsolute);
            return new ResourceDictionary { Source = foo };
        }

        public override void ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("colortheme", GetResource());
        }

        public override string Group
        {
            get { return Application.Current.Resources["Custom"].ToString(); }
        }
    }
}
