using Hurricane.Designer.Data;
using Hurricane.ViewModelBase;

namespace Hurricane.Designer
{
    public class ThemePackViewModel : PropertyChangedBase
    {
        #region "Singleton & Constructor"

        private static ThemePackViewModel _instance;
        public static ThemePackViewModel Instance
        {
            get { return _instance ?? (_instance = new ThemePackViewModel()); }
        }


        private ThemePackViewModel()
        {
        }

        #endregion

        private ThemePack _themePack;
        public ThemePack ThemePack
        {
            get { return _themePack; }
            set
            {
                SetProperty(value, ref _themePack);
            }
        }
    }
}
