using System;
using System.Collections.Generic;
using System.Linq;
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
        public TestWindow()
        {
            InitializeComponent();
            AddMessageEvent += TestWindow_AddMessageEvent;
        }

        void TestWindow_AddMessageEvent(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                InfoListBox.Items.Add(e);
                InfoListBox.ScrollIntoView(e);
            });
        }

        private static event EventHandler<string> AddMessageEvent;
        public static void AddMessage(string message)
        {
            if (AddMessageEvent != null) AddMessageEvent(null, DateTime.Now.Ticks +": \t" + message);
        }
    }
}
