using System;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes.Visual.BaseThemes
{
    [Serializable, XmlInclude(typeof(BaseTheme)), XmlInclude(typeof(CustomBaseTheme))]
    public abstract class BaseThemeBase : IBaseTheme
    {
        public string Name { set; get; }

        public abstract void ApplyTheme();
        public abstract string TranslatedName { get; }
        public abstract bool UseLightDialogs { get; }

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
    }
}
