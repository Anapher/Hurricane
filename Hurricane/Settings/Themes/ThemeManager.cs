using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class ThemeManager
    {
        public ColorTheme SelectedColorTheme { get; set; }
        public bool UseCustomSpectrumAnalyzerColor { get; set; }
        public string SpectrumAnalyzerHexColor { get; set; }

    [XmlIgnore]
        public Color SpectrumAnalyzerColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SpectrumAnalyzerHexColor)) return Colors.Black;
                return (Color)ColorConverter.ConvertFromString(SpectrumAnalyzerHexColor);
            }
            set
            {
                SpectrumAnalyzerHexColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", value.A, value.R, value.G, value.B);
            }
        }

        private List<ColorTheme> themes;
        [XmlIgnore]
        public List<ColorTheme> Themes
        {
            get
            {
                if (themes == null)
                {
                    themes = new List<ColorTheme> { new ColorTheme() { FileName = "Blue" },
                    new ColorTheme() { FileName = "Green"},
                    new ColorTheme() { FileName = "Magenta"},
                    new ColorTheme() { FileName = "Orange" },
                    new ColorTheme() { FileName = "Red" },
                    new ColorTheme() { FileName = "Siena" }};
                }
                return themes;
            }
        }

        private ResourceDictionary lasttheme;
        public void LoadTheme()
        {
            if (lasttheme != null) Application.Current.Resources.Remove(lasttheme);
            var s = string.Format("/Resources/Themes/{0}.xaml", SelectedColorTheme.FileName);
            lasttheme = new ResourceDictionary() { Source = new Uri(s, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(lasttheme);
            if (UseCustomSpectrumAnalyzerColor)
            {
                Application.Current.Resources["SpectrumAnalyzerBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(SpectrumAnalyzerHexColor));
            }
            else
            {
                Application.Current.Resources["SpectrumAnalyzerBrush"] = Application.Current.FindResource("AccentColorBrush");
            }
        }

        public void LoadStandard()
        {
            this.SelectedColorTheme = Themes[0];
            this.UseCustomSpectrumAnalyzerColor = false;
            this.SpectrumAnalyzerHexColor = null;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ThemeManager;
            if (other == null) return false;
            return this.SelectedColorTheme.FileName == other.SelectedColorTheme.FileName && this.UseCustomSpectrumAnalyzerColor == other.UseCustomSpectrumAnalyzerColor && this.SpectrumAnalyzerColor == other.SpectrumAnalyzerColor;
        }
    }
}
