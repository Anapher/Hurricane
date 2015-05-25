using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hurricane.Model.MusicEqualizer
{
    [Serializable]
    public class EqualizerBandCollection : IReadOnlyCollection<EqualizerBand>
    {
        private static readonly List<string> Bandlabels =
            new List<string>(new[] {"31", "62", "125", "250", "500", "1K", "2K", "4K", "8K", "16K"});
        private List<EqualizerBand> _bands;

        public EqualizerBandCollection()
        {
            Bands = Bandlabels.Select(x => new EqualizerBand(x)).ToList();
        }

        public event EventHandler<EqualizerBandChangedEventArgs> EqualizerBandChanged; 
        
        public List<EqualizerBand> Bands
        {
            get
            {
                return _bands;
            }
            set
            {
                if (value == _bands)
                    return;

                if (_bands != null)
                {
                    foreach (var equalizerBand in _bands)
                        equalizerBand.ValueChanged -= NewBandValueChanged;
                }

                _bands = value;
                if (value != null && value.Count > 0)
                {
                    foreach (var equalizerBand in value)
                        equalizerBand.ValueChanged += NewBandValueChanged;
                }
            }
        }

        private void NewBandValueChanged(object sender, EventArgs e)
        {
            var band = sender as EqualizerBand;
            if (band == null)
                throw new ArgumentException("Invalid sender");

            EqualizerBandChanged?.Invoke(this, new EqualizerBandChangedEventArgs(Bands.IndexOf(band), band.Value, band));
        }

        public int Count => 10;

        public IEnumerator<EqualizerBand> GetEnumerator()
        {
            return _bands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _bands.GetEnumerator();
        }
    }
}