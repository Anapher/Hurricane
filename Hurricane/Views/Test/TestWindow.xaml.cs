using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Hurricane.Views.Test
{
    /// <summary>
    /// Interaktionslogik für TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        protected FileInfo logfile;
        public TestWindow()
        {
            //logfile = new FileInfo("log.txt");
            //LogPath = "Logfile location: " + logfile.FullName;
            InitializeComponent();
            this.Loaded += TestWindow_Loaded;
        }

        [DllImport("user32.dll")]
        static extern int GetClassName(int hWnd, StringBuilder lpClassName, int nMaxCount);

        public Steps CurrentStep { get; set; }
        public string LogPath { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //CloseStream();
            //Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
            hook.Unhook();
        }

        Utilities.ActiveWindowHook hook = new Utilities.ActiveWindowHook();
        void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
                txtStatus.Text = "We'll start easy. Please move your mouse";
                CurrentStep = Steps.StepOne;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Oh, looks like we found the exception. Please submit the log to the programmer";
                AddLineToLog("Error with subscribing MouseHook: " + ex.ToString());
                CloseStream();
            }
             * */

            hook.ActiveWindowChanged += hook_ActiveWindowChanged;
            hook.Hook();
        }

        void hook_ActiveWindowChanged(object sender, IntPtr hwnd)
        {
            //var desktophandle = Utilities.WindowHelper.GetDesktopWindow(Utilities.WindowHelper.DesktopWindow.ProgMan);
            //txt.Text = string.Format("Desktop Handle: {0} | Aktuelles Handle: {1} | Desktop im Vordergrund: {2}", desktophandle, hwnd.ToString(), desktophandle == hwnd);
            const int maxChars = 256;
            bool desktopisactive = false;
            StringBuilder className = new StringBuilder(maxChars);
            if (GetClassName((int)hwnd, className, maxChars) > 0)
            {
                string cName = className.ToString();
                desktopisactive = cName == "Progman" || cName == "WorkerW";
                txt.Text = string.Format("Aktueller Classname: {0}; Ist Desktop: {1}", cName, desktopisactive);
                txt.Foreground = desktopisactive ? Brushes.Green : Brushes.Red;
            }
            else { txt.Text = "Fehler"; }
            
        }

        void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (CurrentStep)
            {
                case Steps.StepOne:
                    AddLineToLog("Can move mouse");
                    txtStatus.Text = "Now, please move your mouse to the left side of your screen";
                    CurrentStep = Steps.StepTwo;
                    break;
                case Steps.StepTwo:
                    txtStatus.Text = string.Format("Now, please move your mouse to the left side of your screen (Current X: {0})", e.X);
                    if (e.X == 0)
                    {
                        txtStatus.Text = "Nice one. At least, we want to check if you could see the magic arrow. Please tell me, is this window at the exact left side of your screen and can you see the complete window ?";
                        this.Left = 0;
                        AddLineToLog("Can move mouse to X=0");
                        CurrentStep = Steps.StepThree;
                        btnOk.Visibility = System.Windows.Visibility.Visible;
                        btnDoesntWork.Content = "No";
                    }
                    break;
            }
        }

        protected StreamWriter stream;
        protected void AddLineToLog(string line)
        {
            if (stream == null) stream = new StreamWriter(logfile.FullName);
            stream.WriteLine(line);
        }

        protected void CloseStream()
        {
            if (stream == null) return;
            stream.Flush();
            stream.Close();
        }

        public enum Steps
        {
            StepOne, //Move mouse
            StepTwo, //Move mouse to left side
            StepThree //Check if the user could the the magic arrow
        }

        private void btnDoesntWork_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentStep)
            {
                case Steps.StepOne:
                    AddLineToLog("Can't move mouse");
                    break;
                case Steps.StepTwo:
                    AddLineToLog("Can't move mouse to the left side");
                    break;
                case Steps.StepThree:
                    AddLineToLog("Left is not left");
                    btnDoesntWork.IsEnabled = false;
                    btnOk.IsEnabled = false;
                    txtStatus.Text = "Please give the programmer your log.xml";
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            AddLineToLog("Sees left window: check");
            txtStatus.Text = "Ok, the magic arrow should work. If it's not working, please contact the programmer.";
            btnDoesntWork.IsEnabled = false;
            btnOk.IsEnabled = false;
        }
    }
}
