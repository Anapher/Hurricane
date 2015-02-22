using System;

namespace Hurricane.Designer.Data
{
    public class ThemeBoolean : IThemeSetting
    {
        private bool _booleanValue;
        public string DisplayName { get; set; }
        public string ID { get; set; }
        public string RegexPattern { get; set; }

        public bool BooleanValue
        {
            get { return _booleanValue; }
            set
            {
                _booleanValue = value;
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        public string Value
        {
            get { return BooleanValue.ToString().ToLower(); }
        }

        public void SetValue(string content)
        {
            BooleanValue = bool.Parse(content);
        }


        public event EventHandler ValueChanged;
    }
}
