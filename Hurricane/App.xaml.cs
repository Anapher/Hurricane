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
    /// Interaktionslogik für "App.xaml"
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
                TcpClient client = new TcpClient();

                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
                client.Connect(serverEndPoint);
                NetworkStream clientStream = client.GetStream();
                BinaryWriter writer = new BinaryWriter(clientStream);
                BinaryReader reader = new BinaryReader(clientStream);
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes("gh");
                writer.Write(buffer.Length);
                writer.Write(buffer);
                int length = reader.ReadInt32();
                IntPtr hwnd = new IntPtr(int.Parse(System.Text.ASCIIEncoding.ASCII.GetString(reader.ReadBytes(length))));
                SendMessage(hwnd, BringTheWindowToFrontMessage, IntPtr.Zero, IntPtr.Zero);
                clientStream.Close();
                client.Close();
                App.Current.Shutdown();
            }

            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri("/Resources/Languages/Hurricane.de-de.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
