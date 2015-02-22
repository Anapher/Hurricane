using System;
using System.Windows;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes.Visual.BaseThemes
{
    [Serializable]
    public class BaseTheme : BaseThemeBase
    {
        public override string TranslatedName
        {
            get { return Application.Current.Resources[Name].ToString(); }
        }

        public override string Group
        {
            get { return Application.Current.Resources["Default"].ToString(); }
        }

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary
        {
            get { return new ResourceDictionary { Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Name), UriKind.Relative) }; }
        }
    }
}