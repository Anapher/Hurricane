using System;
using System.Windows;

namespace Hurricane.Settings.Themes.Visual.BaseThemes
{
    [Serializable]
    public class BaseTheme : BaseThemeBase
    {
        public override void ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("basetheme", GetResource());
        }

        private ResourceDictionary GetResource()
        {
            return new ResourceDictionary { Source = new Uri(string.Format("/Resources/Themes/{0}.xaml", Name), UriKind.Relative) };
        }

        public override string TranslatedName
        {
            get { return Application.Current.Resources[Name].ToString(); }
        }

        public override bool UseLightDialogs
        {
            get { return (bool) GetResource()["UseDialogsForWhiteTheme"]; }
        }

        public override string Group
        {
            get { return Application.Current.Resources["Default"].ToString(); }
        }
    }
}
