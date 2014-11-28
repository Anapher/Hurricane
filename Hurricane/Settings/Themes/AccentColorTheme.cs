using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class AccentColorTheme
    {
        public string Name { get; set; }

        [XmlIgnore]
        public Brush BorderColorBrush { get; set; }

        [XmlIgnore]
        public Brush ColorBrush
        {
            get
            {
                var item = ThemeManager.Accents.Where((x) => x.Name == this.Name).First();
                return item.Resources["AccentColorBrush"] as Brush;
            }
        }

        [XmlIgnore]
        public string TranslatedName
        {
            get
            {
                return System.Windows.Application.Current.FindResource(Name).ToString();
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

        public void ApplyTheme()
        {
            var s = string.Format("/Resources/Themes/{0}.xaml", this.Name);
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(s, UriKind.Relative) });
        }
    }
}
