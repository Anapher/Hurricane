using System;

namespace Hurricane.Views.Test
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow
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
