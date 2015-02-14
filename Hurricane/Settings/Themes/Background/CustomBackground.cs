using System;
using System.IO;
using System.Windows.Media.Imaging;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings.Themes.Background
{
    public class CustomBackground : PropertyChangedBase, IBackgroundImage
    {
        protected bool Equals(CustomBackground other)
        {
            if (other.BackgroundPath == null && BackgroundPath == null) return true;
            return string.Equals(BackgroundPath, other.BackgroundPath);
        }

        public override int GetHashCode()
        {
            return (BackgroundPath != null ? BackgroundPath.GetHashCode() : 0);
        }

        private string _backgroundPath;
        public string BackgroundPath
        {
            get { return _backgroundPath; }
            set
            {
                SetProperty(value, ref _backgroundPath);
            }
        }

        public BitmapImage GetBackgroundImage()
        {
            return new BitmapImage(new Uri(BackgroundPath));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = obj as CustomBackground;
            return Equals(other);
        }

        public bool IsAnimated
        {
            get { return BackgroundPath.EndsWith(".gif"); }
        }

        public bool IsAvailable
        {
            get { return File.Exists(BackgroundPath); }
        }


        public string DisplayText
        {
            get { return BackgroundPath; }
        }
    }

    public enum TextColor
    {
        Black,
        White,
        Normal
    }
}