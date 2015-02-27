using System.Windows;
using Hurricane.Designer.Data.ThemeData;
using Hurricane.ViewModelBase;

namespace Hurricane.Designer.Data
{
    public class PreviewData : PropertyChangedBase, IPreviewable
    {
        public AppThemeData AppThemeData { get; private set; }
        public AccentColorData AccentColorData { get; private set; }

        public PreviewData(AccentColorData accentColor, AppThemeData appTheme)
        {
            AccentColorData = accentColor;
            AppThemeData = appTheme;
            foreach (var themeSetting in AccentColorData.ThemeSettings)
            {
                themeSetting.ValueChanged += themeSetting_ValueChanged;
            }

            foreach (var themeSetting in AppThemeData.ThemeSettings)
            {
                themeSetting.ValueChanged += themeSetting_ValueChanged;
            }
        }

        public FrameworkElement FrameworkElement { get; set; }

        private ResourceDictionary _lastColorResourceDictionary;
        private ResourceDictionary _lastBaseResourceDictionary;

        void themeSetting_ValueChanged(object sender, System.EventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            var accentColorResources = AccentColorData.GetResourceDictionary();
            var appThemeResources = AppThemeData.GetResourceDictionary();
            FrameworkElement.Resources.MergedDictionaries.Add(accentColorResources);
            FrameworkElement.Resources.MergedDictionaries.Add(appThemeResources);

            if (_lastColorResourceDictionary != null)
                FrameworkElement.Resources.MergedDictionaries.Remove(_lastColorResourceDictionary);

            if (_lastBaseResourceDictionary != null)
                FrameworkElement.Resources.MergedDictionaries.Remove(_lastBaseResourceDictionary);

            _lastBaseResourceDictionary = appThemeResources;
            _lastColorResourceDictionary = accentColorResources;
        }
    }
}