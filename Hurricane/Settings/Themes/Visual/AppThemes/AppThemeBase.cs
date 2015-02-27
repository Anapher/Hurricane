using System;
using System.Windows;
using System.Xml.Serialization;
using Hurricane.Settings.Converter;

namespace Hurricane.Settings.Themes.Visual.AppThemes
{
    [Serializable, XmlInclude(typeof(AppTheme)), XmlInclude(typeof(CustomAppTheme))]
    public abstract class AppThemeBase : IAppTheme, IGroupable
    {
        public string Name { set; get; }
        public abstract string TranslatedName { get; }
        public abstract string Group { get; }
        [XmlIgnore]
        public abstract ResourceDictionary ResourceDictionary { get; }

        public override bool Equals(object obj)
        {
            var other = obj as AppThemeBase;
            if (other == null) return false;
            if (obj.GetType() == GetType())
                return other.Name == Name;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("apptheme", ResourceDictionary);
        }
    }
}
