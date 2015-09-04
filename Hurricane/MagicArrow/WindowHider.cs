using System.Windows;

namespace Hurricane.MagicArrow
{
    class WindowHider
    {
        private Window _helperWindow;

        public void HideWindowFromAltTab(Window window)
        {
            _helperWindow = new Window
            {
                Top = -100,
                Left = -100,
                Width = 1,
                Height = 1,
                WindowStyle = WindowStyle.ToolWindow
            };
            _helperWindow.Show();
            window.Owner = _helperWindow;
            _helperWindow.Hide();
        }

        public void ShowWindowInAltTab(Window window)
        {
            window.Owner = null;
            _helperWindow.Close();
            _helperWindow = null;
        }
    }
}
