using System.Windows;

namespace Hurricane.Settings.Themes.Visual
{
    public interface IBaseTheme
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
        ResourceDictionary ResourceDictionary { get; }
    }

    public interface IColorTheme
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
        ResourceDictionary ResourceDictionary { get; }
    }
}