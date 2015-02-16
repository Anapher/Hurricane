using System.Windows;
using System.Windows.Controls;
using Hurricane.Designer.Data;
using Hurricane.Designer.Pages;
using Hurricane.ViewModelBase;

namespace Hurricane.Designer
{
    public class DesignerViewModel : PropertyChangedBase
    {
        #region "Singleton & Constructor"

        private static DesignerViewModel _instance;
        public static DesignerViewModel Instance
        {
            get { return _instance ?? (_instance = new DesignerViewModel()); }
        }


        private DesignerViewModel()
        {
        }

        #endregion

        
        private object _currentElement;
        public object CurrentElement
        {
            get { return _currentElement; }
            set
            {
                SetProperty(value, ref _currentElement);
            }
        }
        
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get { return _currentView ?? (_currentView = new HomePage()); }
            set
            {
                SetProperty(value, ref _currentView);   
            }
        }

        
        private IPreviewable _previewData;
        public IPreviewable PreviewData
        {
            get { return _previewData; }
            set
            {
                SetProperty(value, ref _previewData);
            }
        }

        private RelayCommand _createNewThemePack;
        public RelayCommand CreateNewThemePack
        {
            get
            {
                return _createNewThemePack ?? (_createNewThemePack = new RelayCommand(parameter =>
                {
                    ThemePackViewModel.Instance.ThemePack = new ThemePack();
                    CurrentView = new ThemePackPage();
                }));
            }
        }

        public string CurrentMode { get; set; }

        private RelayCommand _createNewBaseColor;
        public RelayCommand CreateNewBaseColor
        {
            get
            {
                return _createNewBaseColor ?? (_createNewBaseColor = new RelayCommand(parameter =>
                {
                    CurrentMode = Application.Current.Resources["BaseTheme"].ToString();
                    CurrentElement = new BaseThemeData();
                    CurrentView = new ThemePage();
                }));
            }
        }
    }
}