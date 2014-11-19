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

namespace Hurricane
{
    /// <summary>
    /// Interaction logic for "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        const int BringTheWindowToFrontMessage = 3532;
        Mutex myMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "Hurricane", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                IntPtr hwnd = FindWindow(null, "Hurricane");
                SendMessage(hwnd, BringTheWindowToFrontMessage, IntPtr.Zero, IntPtr.Zero);
                App.Current.Shutdown();
            }

            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("/Resources/Languages/Hurricane.de-de.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(dict);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            myMutex.Dispose();
        }
    }
}
