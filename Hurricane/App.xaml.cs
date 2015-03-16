using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Hurricane.Notification.WindowMessages;
using Hurricane.Settings;
using Hurricane.Settings.RegistryManager;
using Hurricane.Settings.Themes;
using Hurricane.Utilities;
using Hurricane.Utilities.Native;
using Hurricane.ViewModels;
using Hurricane.Views;
using Hurricane.Views.Test;
using Hurricane.Views.Tools;

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for "App.xaml"
    /// </summary>
    public partial class App
    {
        Mutex _myMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HurricaneSettings.Instance.Load();

            var openfile = false;
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                switch (Environment.GetCommandLineArgs()[1])
                {
                    case "/test":
                        var view = new TestWindow();
                        view.Show();
                        break;
                    case "/language_creator":
                        var languageCreator = new LanguageCreatorWindow();
                        languageCreator.ShowDialog();
                        return;
                    case "/designer":
                        var resource = new ResourceDictionary { Source = new Uri("/Resources/Themes/Cyan.xaml", UriKind.Relative) };
                        ApplicationThemeManager.Instance.LoadResource("accentcolor", resource);
                        var designer = new Designer.DesignerWindow();
                        designer.ShowDialog();
                        return;
                    case "/registry":
                        var manager = new RegistryManager();
                        var item = manager.ContextMenuItems.First(x => x.Extension == Environment.GetCommandLineArgs()[2]);
                        try
                        {
                            if (item != null) item.ToggleRegister(!item.IsRegistered, false);
                        }
                        catch (SecurityException)
                        {
                            MessageBox.Show("Something went extremly wrong. This application didn't got administrator rights so it can't register anything.");
                        }

                        Current.Shutdown();
                        return;
                    case "/screeninfo":
                        var message = new StringBuilder();
                        var screens = WpfScreen.AllScreens().ToList();
                        message.AppendLine("Hurricane - Detected Screens");
                        message.AppendLine("-----------------------------------------------------------------------------------");
                        message.AppendFormat("Found screens: {0}", screens.Count);
                        message.AppendLine();
                        foreach (var wpfScreen in screens)
                        {
                            message.AppendFormat("Screen #{0} ({1})", screens.IndexOf(wpfScreen), wpfScreen.DeviceName);
                            message.AppendFormat("Size: Width {0}, Height {1}", wpfScreen.WorkingArea.Width, wpfScreen.WorkingArea.Height);
                            message.AppendFormat("Position: X {0}, Y {1}", wpfScreen.WorkingArea.X, wpfScreen.WorkingArea.Y);
                            message.AppendFormat("IsPrimary: {0}", wpfScreen.IsPrimary);
                            message.AppendLine();
                        }
                        message.AppendLine("-----------------------------------------------------------------------------------");
                        message.AppendLine();
                        message.AppendFormat("Most left x: {0}", WpfScreen.MostLeftX);
                        message.AppendFormat("Most right x: {0}", WpfScreen.MostRightX);
                        MessageBox.Show(message.ToString());
                        Current.Shutdown();
                        return;
                    case "/showlines":
                        MagicArrow.StrokeWindow.ShowLines = true;
                        break;
                    case "/positiontest":
                        PositionTestWindow positionTest = new PositionTestWindow();
                        positionTest.Show();
                        return;
                    default:
                        openfile = true;
                        break;
                }
            }

            bool aIsNewInstance;
            _myMutex = new Mutex(true, "Hurricane", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, "Hurricane");
                if (openfile)
                {
                    WindowMessanger.SendMessageToWindow(hwnd, WindowMessanger.WM_OPENMUSICFILE, new FileInfo(Environment.GetCommandLineArgs()[1]).FullName);
                }
                else
                {
                    WindowMessanger.SendMessageToWindow(hwnd, WindowMessanger.WM_BRINGTOFRONT, string.Empty);
                }
                Current.Shutdown();
                return;
            }
#if !DEBUG
            EnableExteptionless();
#endif
            //We remove the two last resource dictionarys so we can skip the annyoing ThemeManagers/current theme detections, ...
            //HurricaneSettings will load the needed resources
            Resources.MergedDictionaries.RemoveAt(Resources.MergedDictionaries.Count -2);
            Resources.MergedDictionaries.RemoveAt(Resources.MergedDictionaries.Count - 2);
            ApplicationThemeManager.Instance.Apply(HurricaneSettings.Instance.Config.ApplicationDesign);

            MainWindow window = new MainWindow();

            WindowMessanger messanger = new WindowMessanger(window);
            window.Show();
            if (openfile)
            {
                try
                {
                    foreach (var path in Environment.GetCommandLineArgs().Skip(1))
                    {
                        FileInfo fi = new FileInfo(path);
                        if (fi.Exists)
                        {
                            MainViewModel.Instance.OpenFile(fi, Environment.GetCommandLineArgs().Skip(1).Last() == path);
                        }
                    }
                }
                catch (IOException) { }
            }

            messanger.BringWindowToFront += (s, ev) =>
            {
                window.BringToFront();
            };

            messanger.PlayMusicFile += (s, ev) =>
            {
                FileInfo fi = new FileInfo(ev.Filename);
                if (fi.Exists)
                {
                    MainViewModel.Instance.OpenFile(fi, true);
                }
            };
        }

        protected void EnableExteptionless()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            OnExceptionOccurred(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            OnExceptionOccurred((Exception)e.ExceptionObject);
        }

        bool _isHandled;
        protected void OnExceptionOccurred(Exception ex)
        {
            if (!_isHandled)
            {
                _isHandled = true;
                if (MainViewModel.Instance.MusicManager != null && MainViewModel.Instance.MusicManager.CSCoreEngine.IsPlaying)
                    MainViewModel.Instance.MusicManager.CSCoreEngine.StopPlayback();
                ReportExceptionWindow window = new ReportExceptionWindow(ex);
                window.ShowDialog();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_myMutex != null) _myMutex.Dispose();
        }
    }
}
