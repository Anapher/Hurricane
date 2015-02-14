using System.Collections.Generic;

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
                    ID = "AccentSelectedColor",
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

        public override string Source
        {
            get { return Properties.Resources.ColorTheme; }
        }
    }
}