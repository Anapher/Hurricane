using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace Hurricane.Settings.Themes.Visual.BaseThemes
{
    [Serializable]
    public class CustomBaseTheme : BaseThemeBase
    {
        public static bool FromFile(string filename, out CustomBaseTheme result)
        {
            var baseTheme = new CustomBaseTheme { Name = Path.GetFileNameWithoutExtension(filename) };

            try
            {
                baseTheme.GetResource();
            }
            catch (XamlParseException)
            {
                result = null;
                return false;
            }

            result = baseTheme;
            return true;
        }

        private ResourceDictionary GetResource()
        {
            return new ResourceDictionary { Source = new Uri(Path.Combine(HurricaneSettings.Instance.BaseThemesDirectory, Name + ".xaml"), UriKind.RelativeOrAbsolute) };
        }

        public override void ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("basetheme", GetResource());
        }

        public override string TranslatedName
        {
            get { return Name; }
        }

        public override bool UseLightDialogs
        {
            get { return (bool)GetResource()["UseDialogsForWhiteTheme"]; }
        }
    }
}