using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.MusicEqualizer
{
    [Serializable]
    public class EqualizerSettings : PropertyChangedBase
    {
        public event EventHandler<EqualizerChangedEventArgs> EqualizerChanged;

        protected List<string> bandlabels = new List<string>(new string[] { "31", "62", "125", "250", "500", "1K", "2K", "4K", "8K", "16K" });

        public void CreateNew()
        {
            if (Bands != null) { Bands.Clear(); } else { Bands = new ObservableCollection<EqualizerBand>(); }

            for (int i = 0; i < 10; i++)
            {
                Bands.Add(new EqualizerBand(bandlabels[i]));
            }
            Loaded();
        }

        public void Loaded()
        {
            foreach (EqualizerBand b in Bands)
            {
                b.EqualizerChanged += (s, e) =>
                {
                    if (EqualizerChanged != null)
                        EqualizerChanged(this, new EqualizerChangedEventArgs(Bands.IndexOf(b), b.Value));
                };
                b.Label = bandlabels[Bands.IndexOf(b)];
            }
        }

        private RelayCommand _resetequalizer;
        public RelayCommand ResetEqualizer
        {
            get
            {
                return _resetequalizer ?? (_resetequalizer = new RelayCommand(parameter =>
                {
                    foreach (EqualizerBand band in Bands)
                    {
                        band.Value = 0;
                    }
                }));
            }
        }

        private ObservableCollection<EqualizerBand> _bands;
        public ObservableCollection<EqualizerBand> Bands
        {
            get { return _bands; }
            set
            {
                SetProperty(value, ref _bands);
            }
        }

        private RelayCommand _loadpresetbass;
        public RelayCommand LoadPresetBass
        {
            get { return _loadpresetbass ?? (_loadpresetbass = new RelayCommand(parameter => { LoadPreset(50, 35, 20, 5, -10, -10, 0, -2, 0, 2); })); }
        }

        private RelayCommand _loadpresetbassexteme;
        public RelayCommand LoadPresetBassExteme
        {
            get { return _loadpresetbassexteme ?? (_loadpresetbassexteme = new RelayCommand(parameter => { LoadPreset(90, 85, 65, 30, 0, -5, -5, 0, 2, 0); })); }
        }

        private RelayCommand _loadpresetbassandheights;
        public RelayCommand LoadPresetBassAndHeights
        {
            get { return _loadpresetbassandheights ?? (_loadpresetbassandheights = new RelayCommand(parameter => { LoadPreset(20, 20, 10, 0, -10, -10, 0, 5, 20, 20); })); }
        }

        private RelayCommand _loadpresetheights;
        public RelayCommand LoadPresetHeights
        {
            get { return _loadpresetheights ?? (_loadpresetheights = new RelayCommand(parameter => { LoadPreset(-10, -10, -10, -10, -5, -5, 0, 25, 50, 75); })); }
        }

        private RelayCommand _loadpresetclassic;
        public RelayCommand LoadPresetClassic
        {
            get { return _loadpresetclassic ?? (_loadpresetclassic = new RelayCommand(parameter => { LoadPreset(0, 0, 0, 0, 0, 0, -10, -10, -10, -20); })); }
        }

        private RelayCommand _loadpresetdance;
        public RelayCommand LoadPresetDance
        {
            get { return _loadpresetdance ?? (_loadpresetdance = new RelayCommand(parameter => { LoadPreset(30, 15, 10, 0, 0, -10, -5, -5, 0, 0); })); }
        }

        private RelayCommand _loadpresetrock;
        public RelayCommand LoadPresetRock
        {
            get { return _loadpresetrock ?? (_loadpresetrock = new RelayCommand(parameter => { LoadPreset(25, 10, 5, -10, -5, 5, 10, 20, 20, 20); })); }
        }

        private RelayCommand _loadpresettechno;
        public RelayCommand LoadPresetTechno
        {
            get { return _loadpresettechno ?? (_loadpresettechno = new RelayCommand(parameter => { LoadPreset(20, 20, 0, -10, -5, 0, 10, 40, 40, 40); })); }
        }

        private RelayCommand _loadpresetpop;
        public RelayCommand LoadPresetPop
        {
            get { return _loadpresetpop ?? (_loadpresetpop = new RelayCommand(parameter => { LoadPreset(5, 20, 25, 20, 5, 0, 0, 0, 5, 10); })); }
        }

        private RelayCommand _loadpresetsoftbass;
        public RelayCommand LoadPresetSoftBass
        {
            get { return _loadpresetsoftbass ?? (_loadpresetsoftbass = new RelayCommand(parameter => { LoadPreset(10, 10, 10, 0, -10, -10, 0, 0, 0, 0); })); }
        }

        private RelayCommand _loadpresetsoftheights;
        public RelayCommand LoadPresetSoftHeights
        {
            get { return _loadpresetsoftheights ?? (_loadpresetsoftheights = new RelayCommand(parameter => { LoadPreset(0, 0, -20, -20, -20, -20, -10, 0, 10, 20); })); }
        }

        protected void LoadPreset(double zero, double one, double two, double three, double four, double five, double six, double seven, double eight, double nine)
        {
            Bands[0].Value = zero;
            Bands[1].Value = one;
            Bands[2].Value = two;
            Bands[3].Value = three;
            Bands[4].Value = four;
            Bands[5].Value = five;
            Bands[6].Value = six;
            Bands[7].Value = seven;
            Bands[8].Value = eight;
            Bands[9].Value = nine;
        }
    }
}
