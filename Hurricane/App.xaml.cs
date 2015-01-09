using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Exceptionless;
using Hurricane.Notification.WindowMessages;
using Hurricane.Settings;
using Hurricane.Settings.RegistryManager;
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
    public partial class App : Application
    {
        Mutex _myMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var openfile = false;
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                switch (Environment.GetCommandLineArgs()[1])
                {
                    case "/test":
                        TestWindow view = new TestWindow();
                        view.Show();
                        return;
                    case "/language_creator":
                        LanguageCreatorWindow languageCreator = new LanguageCreatorWindow();
                        languageCreator.ShowDialog();
                        return;
                    case "/registry":
                        RegistryManager manager = new RegistryManager();
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
            HurricaneSettings.Instance.Load();
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
                window.MagicArrow.BringToFront();
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

        bool _isHandled = false;
        protected void OnExceptionOccurred(Exception ex)
        {
            if (!_isHandled)
            {
                _isHandled = true;
                ReportExceptionWindow window = new ReportExceptionWindow(ex);
                window.ShowDialog();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (_myMutex != null) _myMutex.Dispose();
            ExceptionlessClient.Current.Dispose();
        }
    }
}
