using System.ComponentModel;
using System.Windows;

namespace Hurricane.Extensions.Controls
{
    /// <summary>
    /// Interaction logic for VolumeIcon.xaml
    /// </summary>
    public partial class VolumeIcon : INotifyPropertyChanged
    {
        public VolumeIcon()
        {
            InitializeComponent();
            RefreshCurrentState();
        }

        public static readonly DependencyProperty CurrentVolumeProperty = DependencyProperty.Register(
            "CurrentVolume", typeof (float), typeof (VolumeIcon), new PropertyMetadata(1.0f, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((VolumeIcon) dependencyObject).RefreshCurrentState();
        }

        void RefreshCurrentState()
        {
            if (CurrentVolume == 0)
            {
                CurrentDisplayState = DisplayState.Mute;
            }
            else if (CurrentVolume <= 0.5)
            {
                CurrentDisplayState = DisplayState.Medium;
            }
            else if (CurrentVolume > 0.5)
            {
                CurrentDisplayState = DisplayState.Loud;
            }
        }

        public float CurrentVolume
        {
            get { return (float) GetValue(CurrentVolumeProperty); }
            set { SetValue(CurrentVolumeProperty, value); }
        }

        private DisplayState _currentDisplayState;
        public DisplayState CurrentDisplayState
        {
            get { return _currentDisplayState; }
            set
            {
                if (_currentDisplayState != value)
                {
                    _currentDisplayState = value;
                    if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("CurrentDisplayState"));
                }
            }
        }

        public enum DisplayState
        {
            Mute, Medium, Loud
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
