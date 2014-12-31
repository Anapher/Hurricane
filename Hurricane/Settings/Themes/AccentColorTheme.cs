using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using MahApps.Metro;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class AccentColorTheme : ThemeBase
    {
        public override string Name { get; set; }

        [XmlIgnore]
        public Brush BorderColorBrush { get; set; }

        [XmlIgnore]
        public override Brush ColorBrush
        {
            get
            {
                var item = ThemeManager.Accents.First(x => x.Name == this.Name);
                return item.Resources["AccentColorBrush"] as Brush;
            }
        }

        [XmlIgnore]
        public override string TranslatedName
        {
            get
            {
                return Application.Current.FindResource(Name).ToString();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as AccentColorTheme;
            if (other == null) return false;
            return other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void ApplyTheme()
        {
            var resource = new ResourceDictionary() { Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", this.Name), UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(resource);
            ApplicationThemeManager.RegisterTheme(resource);
        }

        public override bool IsEditable
        {
            get { return false; }
        }
    }
}
