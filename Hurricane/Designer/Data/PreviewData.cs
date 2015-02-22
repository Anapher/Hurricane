using System.Windows;
using Hurricane.ViewModelBase;

namespace Hurricane.Designer.Data
{
    public class PreviewData : PropertyChangedBase, IPreviewable
    {
        public BaseThemeData BaseThemeData { get; private set; }
        public ColorThemeData ColorThemeData { get; private set; }

        public PreviewData(ColorThemeData colorTheme, BaseThemeData baseTheme)
        {
            ColorThemeData = colorTheme;
            BaseThemeData = baseTheme;
            foreach (var themeSetting in ColorThemeData.ThemeSettings)
            {
                themeSetting.ValueChanged += themeSetting_ValueChanged;
            }

            foreach (var themeSetting in BaseThemeData.ThemeSettings)
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
            var colorThemeResources = ColorThemeData.GetResourceDictionary();
            var baseThemeResources = BaseThemeData.GetResourceDictionary();
            FrameworkElement.Resources.MergedDictionaries.Add(colorThemeResources);
            FrameworkElement.Resources.MergedDictionaries.Add(baseThemeResources);

            if (_lastColorResourceDictionary != null)
                FrameworkElement.Resources.MergedDictionaries.Remove(_lastColorResourceDictionary);

            if (_lastBaseResourceDictionary != null)
                FrameworkElement.Resources.MergedDictionaries.Remove(_lastBaseResourceDictionary);

            _lastBaseResourceDictionary = baseThemeResources;
            _lastColorResourceDictionary = colorThemeResources;
        }
    }
}