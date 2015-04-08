using System;
using System.IO;
using Hurricane.Utilities;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings.Themes.Background
{
    public class CustomApplicationBackground : PropertyChangedBase, IApplicationBackground
    {
        protected bool Equals(CustomApplicationBackground other)
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

        public Uri GetBackground()
        {
            return new Uri(BackgroundPath);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var other = obj as CustomApplicationBackground;
            return Equals(other);
        }

        public bool IsAnimated
        {
            get { return GeneralHelper.IsVideo(BackgroundPath); }
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