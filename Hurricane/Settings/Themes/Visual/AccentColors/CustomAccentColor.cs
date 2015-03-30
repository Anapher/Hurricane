using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

namespace Hurricane.Settings.Themes.Visual.AccentColors
{
    [Serializable]
    public class CustomAccentColor : AccentColorBase
    {
        public static bool FromFile(string filename, out CustomAccentColor result)
        {
            var accentColor = new CustomAccentColor { Name = new FileInfo(filename).Name };

            try
            {
                if (!ThemeManager.IsAccentDictionary(accentColor.ResourceDictionary))
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

            result = accentColor;
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
                        new Uri(Path.Combine(HurricaneSettings.Paths.AccentColorsDirectory, Name),
                            UriKind.RelativeOrAbsolute)
                };
            }
        }
    }
}
