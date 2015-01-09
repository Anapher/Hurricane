using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hurricane.Views.UserControls
{
    /// <summary>
    /// Interaction logic for EqualizerView.xaml
    /// </summary>
    public partial class EqualizerView : INotifyPropertyChanged
    {
        public event EventHandler WantClose;
        public Thickness SliderThickness { get; set; }

        private int _itemSpace;
        public int ItemSpace
        {
            get
            {
                return _itemSpace;
            }
            set
            {
                _itemSpace = value;
                SliderThickness = new Thickness(value, 0, value, 0);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SliderThickness"));
            }
        }

        public EqualizerView()
        {
            InitializeComponent();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            OnWantClose();
        }

        protected void OnWantClose()
        {
            if (WantClose != null) WantClose(this, EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) OnWantClose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
