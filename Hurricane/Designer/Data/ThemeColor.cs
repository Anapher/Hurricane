using System;
using System.Windows.Media;

namespace Hurricane.Designer.Data
{
    public class ThemeColor : IThemeSetting
    {
        private Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        public string RegexPattern { get; set; }
        public bool IsTransparencyEnabled { get; set; }

        public ThemeColor()
        {
            Color = Color.FromArgb(255, 0, 0, 0);
            IsTransparencyEnabled = true;
        }

        public string DisplayName { get; set; }

        public string ID { get; set; }

        public string Value
        {
            get { return ColorToString(Color, IsTransparencyEnabled); }
        }

        private string ColorToString(Color c, bool withTransparencyValue = true)
        {
            return withTransparencyValue ? string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B) : string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        public void SetValue(string content)
        {
            var newColor = ColorConverter.ConvertFromString(content);
            if (newColor == null) return;
            Color = (Color)newColor;
        }

        public event EventHandler ValueChanged;
    }
}