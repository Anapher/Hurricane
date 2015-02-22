using System;
using System.Windows;
using System.Xml.Serialization;
using Hurricane.Settings.Converter;

namespace Hurricane.Settings.Themes.Visual.BaseThemes
{
    [Serializable, XmlInclude(typeof(BaseTheme)), XmlInclude(typeof(CustomBaseTheme))]
    public abstract class BaseThemeBase : IBaseTheme, IGroupable
    {
        public string Name { set; get; }
        public abstract string TranslatedName { get; }
        public abstract string Group { get; }
        [XmlIgnore]
        public abstract ResourceDictionary ResourceDictionary { get; }

        public override bool Equals(object obj)
        {
            var other = obj as BaseThemeBase;
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
            ApplicationThemeManager.Instance.LoadResource("basetheme", ResourceDictionary);
        }
    }
}
