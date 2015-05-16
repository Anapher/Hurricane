using System;

namespace Hurricane.Model.MusicEqualizer
{
    [Serializable]
    public class EqualizerBand : PropertyChangedBase
    {
        private double _value;

        public event EventHandler ValueChanged;

        public double Value
        {
            get { return _value; }
            set
            {
                if (SetProperty(value, ref _value))
                    ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Label { get; set; }

        public EqualizerBand(string label)
        {
            Label = label;
        }

        //For xml-deserialization
        private EqualizerBand()
        {
        }
    }
}