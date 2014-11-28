using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows;
using MahApps.Metro;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class ApplicationThemeManager
    {
        public AccentColorTheme SelectedColorTheme { get; set; }
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

        private List<AccentColorTheme> themes;
        [XmlIgnore]
        public List<AccentColorTheme> Themes
        {
            get
            {
                if (themes == null)
                {
                    themes = ThemeManager.Accents.Select(a => new AccentColorTheme() { Name = a.Name }).OrderBy((x) => x.TranslatedName).ToList(); ;
                }
                return themes;
            }
        }

        public void LoadTheme()
        {
            SelectedColorTheme.ApplyTheme();
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
            this.SelectedColorTheme = Themes.Where((x) => x.Name == "Blue").First();
            this.UseCustomSpectrumAnalyzerColor = false;
            this.SpectrumAnalyzerHexColor = null;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ApplicationThemeManager;
            if (other == null) return false;
            return this.SelectedColorTheme.Name == other.SelectedColorTheme.Name && this.UseCustomSpectrumAnalyzerColor == other.UseCustomSpectrumAnalyzerColor && this.SpectrumAnalyzerColor == other.SpectrumAnalyzerColor;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
