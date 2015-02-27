using System;
using System.Windows;
using System.Windows.Controls;
using Hurricane.Designer.Data;
using Hurricane.Designer.Data.ThemeData;
using Hurricane.Designer.Pages;
using Hurricane.Settings.Themes;
using Hurricane.ViewModelBase;
using Microsoft.Win32;

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
            CurrentTitle = "Hurricane Designer";
            ApplicationThemeManager.Instance.Refresh();
        }

        #endregion

        private ISaveable _currentElement;
        public ISaveable CurrentElement
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
            get { return _currentView; }
            set
            {
                SetProperty(value, ref _currentView);   
            }
        }

        private UserControl _previewControl;
        public UserControl PreviewControl
        {
            get { return _previewControl ?? (_previewControl = new HomePage()); }
            set
            {
                SetProperty(value, ref _previewControl);
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
        
        private string _currentTitle;
        public string CurrentTitle
        {
            get { return _currentTitle; }
            set
            {
                SetProperty(value, ref _currentTitle);
            }
        }

        private RelayCommand _createNewAppTheme;
        public RelayCommand CreateNewAppTheme
        {
            get
            {
                return _createNewAppTheme ?? (_createNewAppTheme = new RelayCommand(parameter =>
                {
                    LoadTheme(AccentColorData.LoadDefault(), new AppThemeData(), false);
                }));
            }
        }

        private RelayCommand _createNewAccentColor;
        public RelayCommand CreateNewAccentColor
        {
            get
            {
                return _createNewAccentColor ?? (_createNewAccentColor = new RelayCommand(parameter =>
                {
                    LoadTheme(new AccentColorData(), AppThemeData.LoadDefault(), true);
                }));
            }
        }

        private RelayCommand _openAppTheme;
        public RelayCommand OpenAppTheme
        {
            get
            {
                return _openAppTheme ?? (_openAppTheme = new RelayCommand(parameter =>
                {
                    var theme = new AppThemeData();

                    var ofd = new OpenFileDialog {Filter = theme.Filter, InitialDirectory = theme.BaseDirectory};
                    if (ofd.ShowDialog() == true)
                    {
                        try
                        {
                            theme.LoadFromFile(ofd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                        LoadTheme(AccentColorData.LoadDefault(), theme, false);
                        CurrentElementPath = ofd.FileName;
                    }
                }));
            }
        }

        private RelayCommand _openAccentColor;
        public RelayCommand OpenAccentColor
        {
            get
            {
                return _openAccentColor ?? (_openAccentColor = new RelayCommand(parameter =>
                {
                    var theme = new AccentColorData();

                    var ofd = new OpenFileDialog { Filter = theme.Filter, InitialDirectory = theme.BaseDirectory };
                    if (ofd.ShowDialog() == true)
                    {
                        try
                        {
                            theme.LoadFromFile(ofd.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }
                        LoadTheme(theme, AppThemeData.LoadDefault(), true);
                        CurrentElementPath = ofd.FileName;
                    }
                }));
            }
        }

        private void LoadTheme(AccentColorData accentColor, AppThemeData appTheme, bool editAccentColor)
        {
            CurrentTitle = editAccentColor
                ? Application.Current.Resources["AccentColorString"].ToString()
                : Application.Current.Resources["AppTheme"].ToString();

            var themeToEdit = editAccentColor ? (DataThemeBase) accentColor : appTheme;
            CurrentElement = themeToEdit;
            CurrentView = new ThemePage();
            var previewData = new PreviewData(accentColor, appTheme) { FrameworkElement = PreviewControl };
            PreviewData = previewData;
            PreviewControl = new LivePreview();
            previewData.FrameworkElement = PreviewControl;
            previewData.Refresh();
            CanGoBack = true;
        }

        private RelayCommand _saveCurrentElement;
        public RelayCommand SaveCurrentElement
        {
            get
            {
                return _saveCurrentElement ?? (_saveCurrentElement = new RelayCommand(parameter =>
                {
                    CurrentElement.Save(CurrentElementPath);
                }));
            }
        }

        private RelayCommand _selectSavePath;
        public RelayCommand SelectSavePath
        {
            get
            {
                return _selectSavePath ?? (_selectSavePath = new RelayCommand(parameter =>
                {
                    var sfd = new SaveFileDialog
                    {
                        Filter = CurrentElement.Filter,
                        InitialDirectory = 
                            string.IsNullOrEmpty(CurrentElementPath)
                                ? CurrentElement.BaseDirectory
                                : CurrentElementPath
                    };

                    if (sfd.ShowDialog() == true)
                    {
                        CurrentElementPath = sfd.FileName;
                    }
                }));
            }
        }

        private string _currentElementPath;
        public string CurrentElementPath
        {
            get { return _currentElementPath; }
            set
            {
                SetProperty(value, ref _currentElementPath);
            }
        }

        
        private bool _canGoBack;
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set
            {
                SetProperty(value, ref _canGoBack);
            }
        }

        private RelayCommand _goBack;
        public RelayCommand GoBack
        {
            get
            {
                return _goBack ?? (_goBack = new RelayCommand(parameter =>
                {
                    PreviewControl = null;
                    CurrentElement = null;
                    CurrentView = null;
                    CanGoBack = false;
                    CurrentTitle = "Hurricane Designer";
                    CurrentElementPath = string.Empty;
                    PreviewData = null;
                }));
            }
        }
    }
}