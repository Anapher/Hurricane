using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class CustomColorTheme : ThemeBase
    {
        protected ResourceDictionary resource;

        public bool Load(string filename)
        {
            this.Name = Path.GetFileNameWithoutExtension(filename);

            try
            {
                LoadResource();
            }
            catch (XamlParseException)
            {
                return false;
            }
            
            return true;
        }

        protected void LoadResource()
        {
            if (resource == null)
            {
                var foo = new Uri(string.Format("pack://siteoforigin:,,,/Themes/{0}.xaml", Name), UriKind.RelativeOrAbsolute);
                resource = new ResourceDictionary() { Source = foo };
            }
        }

        private string _name;
        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Filename { get { return Name + ".xaml"; } }

        [XmlIgnore]
        public override string TranslatedName
        {
            get { return Name; }
        }

        [XmlIgnore]
        public override Brush ColorBrush
        {
            get
            {
                LoadResource();
                return resource["AccentColorBrush"] as Brush;
            }
        }

        public override void ApplyTheme()
        {
            LoadResource();
            Application.Current.Resources.MergedDictionaries.Add(resource);
            ApplicationThemeManager.RegisterTheme(resource);
        }

        public void RefreshResource()
        {
            resource = null;
        }

        public override bool IsEditable
        {
            get { return true; }
        }
    }
}
