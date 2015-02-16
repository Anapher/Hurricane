using System;
using System.ComponentModel;
using System.Windows;
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
        public bool ShowLabelBelowSlider { get; set; }

        private bool _showSeparator;
        public bool ShowSeparator
        {
            get { return _showSeparator; }
            set
            {
                _showSeparator = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ShowSeparator"));
            }
        }
        

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

        public static readonly DependencyProperty WantCloseCommandProperty = DependencyProperty.Register(
            "WantCloseCommand", typeof (ICommand), typeof (EqualizerView), new PropertyMetadata(default(ICommand)));

        public ICommand WantCloseCommand
        {
            get { return (ICommand) GetValue(WantCloseCommandProperty); }
            set { SetValue(WantCloseCommandProperty, value); }
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
            if (WantCloseCommand != null) WantCloseCommand.Execute(null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) OnWantClose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
