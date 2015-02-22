using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

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
                if (!ThemeManager.IsAccentDictionary(colorTheme.ResourceDictionary))
                {
                    result = null;
                    return false;
                }
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
            get { return _colorBrush ?? (_colorBrush = ResourceDictionary["AccentColorBrush"] as Brush); }
        }

        public override string Group
        {
            get { return Application.Current.Resources["Custom"].ToString(); }
        }

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary
        {
            get
            {
                return new ResourceDictionary
                {
                    Source =
                        new Uri(Path.Combine(HurricaneSettings.Instance.ColorThemesDirectory, Name),
                            UriKind.RelativeOrAbsolute)
                };
            }
        }
    }
}
