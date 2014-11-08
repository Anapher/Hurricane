using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
    [Serializable]
  public  class EqualizerBand : ViewModelBase.PropertyChangedBase
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
    }
}
