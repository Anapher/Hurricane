using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes.Visual.ColorThemes
{
    [Serializable, XmlInclude(typeof(AccentColorTheme)), XmlInclude(typeof(CustomColorTheme))]
    public abstract class ColorThemeBase : IColorTheme
    {
        public abstract Brush ColorBrush { get; }
        public abstract string TranslatedName { get; }
        public abstract void ApplyTheme();

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as ColorThemeBase;
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