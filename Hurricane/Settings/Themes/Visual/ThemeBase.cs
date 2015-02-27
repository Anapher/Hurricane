using System.Windows;

namespace Hurricane.Settings.Themes.Visual
{
    public interface IAppTheme
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
        ResourceDictionary ResourceDictionary { get; }
    }

    public interface IAccentColor
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
        ResourceDictionary ResourceDictionary { get; }
    }
}