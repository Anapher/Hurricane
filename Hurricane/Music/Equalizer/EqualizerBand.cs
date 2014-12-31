using System;
using System.Xml.Serialization;
using Hurricane.ViewModelBase;

namespace Hurricane.Music
{
    [Serializable]
  public  class EqualizerBand : PropertyChangedBase
    {
        public event EventHandler EqualizerChanged; //We build the full event in the EqualizerSettings, because the EqualizerBand doesn't know his number
        
        private double _value = 0;
        public double Value
        {
            get { return _value; }
            set
            {
                if (SetProperty(value, ref _value))
                {
                   if (EqualizerChanged != null) EqualizerChanged(this, EventArgs.Empty);
                }
            }
        }

        public EqualizerBand(string label)
        {
            this.Label = label;
        }

        public EqualizerBand()
        {

        }

        [XmlIgnore]
        public string Label { get; set; }
    }
}
