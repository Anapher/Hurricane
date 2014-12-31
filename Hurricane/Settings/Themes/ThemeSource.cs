using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Hurricane.Settings.Themes
{
   public class ThemeSource
    {
        public string Name { get; set; }

        public ThemeSource()
        {
            ThemeColors = new List<ThemeColor>
            {
                new ThemeColor()
                {
                    Description = GetResource("LightColorDescription"),
                    RegexPattern = "x:Key=\"LightColorBrush\" Color=\"(?<color>(.*?))\"",
                    Name = "LightColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("BrightColorDescription"),
                    RegexPattern = "x:Key=\"BrightColorBrush\" Color=\"(?<color>(.*?))\"",
                    Name = "BrightColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("NormalColorDescription"),
                    RegexPattern = "x:Key=\"NormalColorBrush\" Color=\"(?<color>(.*?))\"",
                    Name = "NormalColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("DarkColorDescription"),
                    RegexPattern = "x:Key=\"DarkColorBrush\" Color=\"(?<color>(.*?))\"",
                    Name = "DarkColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("HighlightColorDescription"),
                    RegexPattern = "x:Key=\"HighlightColor\">(?<color>(.*?))<",
                    Name = "HighlightColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("AccentColorDescription"),
                    RegexPattern = "x:Key=\"AccentColor\">(?<color>(.*?))<",
                    Name = "AccentColor",
                    IsTransparencyEnabled = false
                },
                new ThemeColor()
                {
                    Description = GetResource("AccentSelectedColorDescription"),
                    RegexPattern = "x:Key=\"AccentSelectedColorBrush\" Color=\"(?<color>(.*?))\"",
                    Name = "AccentSelectedColor"
                },
                new ThemeColor()
                {
                    Description = GetResource("IdealForegroundColorDescription"),
                    RegexPattern = "x:Key=\"IdealForegroundColor\">(?<color>(.*?))<",
                    Name = "IdealForegroundColor"
                }
            };
        }

        protected string GetResource(string resourcename)
        {
            return Application.Current.FindResource(resourcename).ToString();
        }

        public void LoadFromFile(string filename)
        {
            string content = File.ReadAllText(Path.Combine("Themes", filename));
            foreach (var color in this.ThemeColors)
            {
                color.Color = GetColorValue(color.RegexPattern, content);
            }
        }

        public void Save()
        {
            string xamlstring = this.ThemeColors.Aggregate(Properties.Resources.ColorTheme, (current, color) => current.Replace("{" + color.Name + "}", ColorToString(color.Color, color.IsTransparencyEnabled)));
            File.WriteAllText(Path.Combine("Themes", Name + ".xaml"), xamlstring);
        }

        protected Color GetColorValue(string pattern, string input)
        {
            Regex regex = new Regex(pattern);
            var that = regex.Match(input);
            return (Color)ColorConverter.ConvertFromString(that.Groups["color"].Value);
        }

        protected string ColorToString(Color c, bool withTransparencyValue = true)
        {
            if (withTransparencyValue)
            {
               return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
            else { return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B); }
        }

        public List<ThemeColor> ThemeColors { get; set; }
    }

    public class ThemeColor
    {
        public Color Color { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RegexPattern { get; set; }
        public bool IsTransparencyEnabled { get; set; }

        public ThemeColor()
        {
            this.Color = Color.FromArgb(255, 0, 0, 0);
            this.IsTransparencyEnabled = true;
        }
    }
}
