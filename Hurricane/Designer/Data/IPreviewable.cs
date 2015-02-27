using Hurricane.Designer.Data.ThemeData;

namespace Hurricane.Designer.Data
{
    public interface IPreviewable
    {
        AppThemeData AppThemeData { get; }
        AccentColorData AccentColorData { get; }
    }
}