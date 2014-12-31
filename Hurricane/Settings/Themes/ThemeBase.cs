using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes
{
    [Serializable, XmlInclude(typeof(CustomColorTheme)), XmlInclude(typeof(AccentColorTheme))]
    public abstract class ThemeBase
    {
        public abstract void ApplyTheme();
        public abstract string Name { set; get; }
        public abstract string TranslatedName { get; }
        public abstract Brush ColorBrush { get; }
        public abstract bool IsEditable { get; }

        public override bool Equals(object obj)
        {
            var other = obj as ThemeBase;
            if (other == null) return false;
            if (obj.GetType() == this.GetType())
                return other.Name == this.Name;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
