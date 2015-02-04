using System;
using System.Windows;

namespace Hurricane.Views.Test
{
    /// <summary>
    /// Interaktionslogik für PositionTestWindow.xaml
    /// </summary>
    public partial class PositionTestWindow : Window
    {
        public PositionTestWindow()
        {
            InitializeComponent();
            this.LocationChanged += PositionTestWindow_LocationChanged;
            txt.Text = Left.ToString();
        }

        void PositionTestWindow_LocationChanged(object sender, EventArgs e)
        {
            txt.Text = Left.ToString();
        }
    }
}
