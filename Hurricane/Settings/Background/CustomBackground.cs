using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hurricane.Settings.MirrorManagement;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings.Background
{
    public class CustomBackground : PropertyChangedBase
    {
        protected bool Equals(CustomBackground other)
        {
            if (other.BackgroundPath == null && BackgroundPath == null) return true;
            return string.Equals(_backgroundPath, other._backgroundPath);
        }

        public override int GetHashCode()
        {
            return (_backgroundPath != null ? _backgroundPath.GetHashCode() : 0);
        }

        private string _backgroundPath;
        [CopyableProperty]
        public string BackgroundPath
        {
            get { return _backgroundPath; }
            set
            {
                SetProperty(value, ref _backgroundPath);
            }
        }

        public BitmapImage GetImage()
        {
            return new BitmapImage(new Uri(BackgroundPath));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = obj as CustomBackground;
            return Equals(other);
        }
    }

    public enum TextColor
    {
        Black,
        White,
        Normal
    }
}