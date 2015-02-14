namespace Hurricane.Settings.Themes.Visual
{
    public interface IBaseTheme
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
        bool UseLightDialogs { get; }
    }

    public interface IColorTheme
    {
        string Name { get; }
        string TranslatedName { get; }
        void ApplyTheme();
    }
}