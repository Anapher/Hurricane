using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using Exceptionless;
using Hurricane.Utilities.Native;

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        const int BringTheWindowToFrontMessage = 3532;
        Mutex myMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var openfile = false;
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                switch (Environment.GetCommandLineArgs()[1])
                {
                    case "/test":
                        Views.Test.TestWindow view = new Views.Test.TestWindow();
                        view.Show();
                        return;
                    default:
                        openfile = true;
                        break;
                }
            }

            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "Hurricane", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                IntPtr hwnd = UnsafeNativeMethods.FindWindow(null, "Hurricane");
                if (openfile)
                {
                    Notification.WindowMessages.WindowMessanger.SendMessageToWindow(hwnd, Notification.WindowMessages.WindowMessanger.WM_OPENMUSICFILE, new FileInfo(Environment.GetCommandLineArgs()[1]).FullName);
                }
                else
                {
                    Notification.WindowMessages.WindowMessanger.SendMessageToWindow(hwnd, Notification.WindowMessages.WindowMessanger.WM_BRINGTOFRONT, string.Empty);
                }
                App.Current.Shutdown();
                return;
            }
#if !DEBUG
                        EnableExteptionless();
#endif

            Hurricane.MainWindow window = new MainWindow();

            Notification.WindowMessages.WindowMessanger messanger = new Notification.WindowMessages.WindowMessanger(window);
            window.Show();
            if (openfile)
            {
                try
                {
                    FileInfo fi = new FileInfo(Environment.GetCommandLineArgs()[1]);
                    if (fi.Exists)
                    {
                        ViewModels.MainViewModel.Instance.OpenFile(fi);
                    }
                }
                catch (IOException) { }
            }

            messanger.BringWindowToFront += (s, ev) => {
                window.MagicArrow.BringToFront();
            };

            messanger.PlayMusicFile += (s, ev) =>
            {
                FileInfo fi = new FileInfo(ev.Filename);
                if (fi.Exists)
                {
                    ViewModels.MainViewModel.Instance.OpenFile(fi);
                }
            };
        }

        protected void EnableExteptionless()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            OnExceptionOccurred(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            OnExceptionOccurred((Exception)e.ExceptionObject);
        }

        bool IsHandled = false;
        protected void OnExceptionOccurred(Exception ex)
        {
            if (!IsHandled)
            {
                IsHandled = true;
                Views.ReportExceptionWindow window = new Views.ReportExceptionWindow(ex);
                window.ShowDialog();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            myMutex.Dispose();
            ExceptionlessClient.Current.Dispose();
        }
    }
}
