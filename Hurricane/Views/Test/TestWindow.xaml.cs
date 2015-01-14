using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Hurricane.Utilities;
using System.Threading.Tasks;

namespace Hurricane.Views.Test
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
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

        public Steps CurrentStep { get; set; }
        public string LogPath { get; set; }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Utilities.HookManager.MouseHook.HookManager.MouseMove -= HookManager_MouseMove;
            hook.Unhook();
        }

        ActiveWindowHook hook = new ActiveWindowHook();
        async void TestWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatus.Text = "We'll start easy. Please move your mouse";
                await Task.Delay(3000);
                Utilities.HookManager.MouseHook.HookManager.MouseMove += HookManager_MouseMove;
                CurrentStep = Steps.StepOne;
            }
            catch
            {
                txtStatus.Text = "Oh, looks like we found the exception. Please submit the log to the programmer";
            }

        }

        void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            switch (CurrentStep)
            {
                case Steps.StepOne:
                    txtStatus.Text = "Now, please move your mouse to the left side of your screen";
                    CurrentStep = Steps.StepTwo;
                    break;
                case Steps.StepTwo:
                    txtStatus.Text = string.Format("Now, please move your mouse to the left side of your screen (Current X: {0})", e.X);
                    if (e.X == 0)
                    {
                        CurrentStep = Steps.StepThree;
                    }
                    break;
                case Steps.StepThree:
                    txtStatus.Text = "Nice one. Next, please move your mouse to the right side of your screen";
                    if (e.X < WpfScreen.AllScreensWidth - 5)
                    {
                        CurrentStep = Steps.StepFour;
                    }
                    break;
                case Steps.StepFour:
                    
                    break;
            }
        }

        public enum Steps
        {
            StepOne, //Move mouse
            StepTwo, //Move mouse to left side
            StepThree,
            StepFour //Check if the user could the the magic arrow
        }

        private void btnDoesntWork_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentStep)
            {
                case Steps.StepOne:
                    break;
                case Steps.StepTwo:
                    break;
                case Steps.StepThree:
                    btnDoesntWork.IsEnabled = false;
                    btnOk.IsEnabled = false;
                    break;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Ok, the magic arrow should work. If it's not working, please contact the programmer.";
            btnDoesntWork.IsEnabled = false;
            btnOk.IsEnabled = false;
        }
    }
}
