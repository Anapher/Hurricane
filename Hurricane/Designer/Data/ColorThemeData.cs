using System.Collections.Generic;
using System.Linq;
using Hurricane.Settings.Themes;
using System.Windows;
using Hurricane.Settings;

namespace Hurricane.Designer.Data
{
    public class ColorThemeData : DataThemeBase
    {
        public ColorThemeData()
        {
            ThemeSettings = new List<IThemeSetting>
            {
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"LightColor\">(?<content>(.*?))<",
                    ID = "LightColor",
                    DisplayName ="Light color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"BrightColor\">(?<content>(.*?))<",
                    ID = "BrightColor",
                    DisplayName ="Bright color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"NormalColor\">(?<content>(.*?))<",
                    ID = "NormalColor",
                                        DisplayName = "Normal color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"DarkColor\">(?<content>(.*?))<",
                    ID = "DarkColor",
                    DisplayName ="Dark color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"HighlightColor\">(?<content>(.*?))<",
                    ID = "HighlightColor",
                    DisplayName ="Hightlight color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"AccentColor\">(?<content>(.*?))<",
                    ID = "AccentColor",
                    DisplayName ="Accent color",
                    IsTransparencyEnabled = false
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"AccentSelectedColorBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "AccentSelectedColorBrush",
                    DisplayName ="Selected color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"IdealForegroundColor\">(?<content>(.*?))<",
                    ID = "IdealForegroundColor",
                    DisplayName ="Ideal foreground color"
                }
            };
        }

        public static ColorThemeData LoadDefault()
        {
            var colorTheme = new ColorThemeData();
            colorTheme.LoadFromResourceDictionary(ApplicationThemeManager.Instance.ColorThemes.First(x => x.Name == "Cyan").ResourceDictionary);
            return colorTheme;
        }

        public override string Source
        {
            get { return Properties.Resources.ColorTheme; }
        }

        public override string Filter
        {
            get { return string.Format("{0}|*.xaml", Application.Current.Resources["ColorTheme"]); }
        }

        public override string BaseDirectory
        {
            get { return HurricaneSettings.Instance.ColorThemesDirectory; }
        }
    }
}