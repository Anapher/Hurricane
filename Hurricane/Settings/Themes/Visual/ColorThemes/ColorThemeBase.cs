using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Hurricane.Settings.Converter;

namespace Hurricane.Settings.Themes.Visual.ColorThemes
{
    [Serializable, XmlInclude(typeof(AccentColorTheme)), XmlInclude(typeof(CustomColorTheme))]
    public abstract class ColorThemeBase : IColorTheme, IGroupable
    {
        public abstract Brush ColorBrush { get; }
        public abstract string TranslatedName { get; }
        public abstract string Group { get; }
        [XmlIgnore]
        public abstract ResourceDictionary ResourceDictionary { get; }

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

        public void ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("colortheme", ResourceDictionary);
        }
    }
}