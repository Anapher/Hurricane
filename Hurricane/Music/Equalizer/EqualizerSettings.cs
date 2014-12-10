using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Hurricane.ViewModelBase;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace Hurricane.Music
{
    [Serializable]
    public class EqualizerSettings : PropertyChangedBase
    {
        public event EventHandler<EqualizerChangedEventArgs> EqualizerChanged;

        protected List<string> bandlabels = new List<string>(new string[] { "31Hz", "62Hz", "125Hz", "250Hz", "500Hz", "1KHz", "2KHz", "4KHz", "8KHz", "16KHz" });

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
                b.EqualizerChanged += (s, e) => { EqualizerChanged(this, new EqualizerChangedEventArgs(Bands.IndexOf(b), b.Value)); };
                b.Label = bandlabels[Bands.IndexOf(b)];
            }
        }

        private RelayCommand resetequalizer;
        public RelayCommand ResetEqualizer
        {
            get
            {
                if (resetequalizer == null)
                    resetequalizer = new RelayCommand((object parameter) =>
                    {
                        foreach (EqualizerBand band in Bands)
                        {
                            band.Value = 0;
                        }
                    });
                return resetequalizer;
            }
        }

        private ObservableCollection<EqualizerBand> bands;
        public ObservableCollection<EqualizerBand> Bands
        {
            get { return bands; }
            set
            {
                SetProperty(value, ref bands);
            }
        }

        private RelayCommand loadpresetbass;
        public RelayCommand LoadPresetBass
        {
            get
            {
                if (loadpresetbass == null)
                    loadpresetbass = new RelayCommand((object parameter) =>
                    {
                        LoadPreset(50, 35, 20, 5, -10, -10, 0, -2, 0, 2);
                    });
                return loadpresetbass;
            }
        }

        private RelayCommand loadpresetbassexteme;
        public RelayCommand LoadPresetBassExteme
        {
            get
            {
                if (loadpresetbassexteme == null)
                    loadpresetbassexteme = new RelayCommand((object parameter) =>
                    {
                        LoadPreset(90, 85, 65, 30, 0, -5, -5, 0, 2, 0);
                    });
                return loadpresetbassexteme;
            }
        }

        private RelayCommand loadpresetbassandheights;
        public RelayCommand LoadPresetBassAndHeights
        {
            get
            {
                if (loadpresetbassandheights == null)
                    loadpresetbassandheights = new RelayCommand((object parameter) => { LoadPreset(20, 20, 10, 0, -10, -10, 0, 5, 20, 20); });
                return loadpresetbassandheights;
            }
        }

        private RelayCommand loadpresetheights;
        public RelayCommand LoadPresetHeights
        {
            get
            {
                if (loadpresetheights == null)
                    loadpresetheights = new RelayCommand((object parameter) => { LoadPreset(-10, -10, -10, -10, -5, -5, 0, 25, 50, 75); });
                return loadpresetheights;
            }
        }

        private RelayCommand loadpresetclassic;
        public RelayCommand LoadPresetClassic
        {
            get
            {
                if (loadpresetclassic == null)
                    loadpresetclassic = new RelayCommand((object parameter) => { LoadPreset(0, 0, 0, 0, 0, 0, -10, -10, -10, -20); });
                return loadpresetclassic;
            }
        }

        private RelayCommand loadpresetdance;
        public RelayCommand LoadPresetDance
        {
            get
            {
                if (loadpresetdance == null)
                    loadpresetdance = new RelayCommand((object parameter) => { LoadPreset(30, 15, 10, 0, 0, -10, -5, -5, 0, 0); });
                return loadpresetdance;
            }
        }

        private RelayCommand loadpresetrock;
        public RelayCommand LoadPresetRock
        {
            get
            {
                if (loadpresetrock == null)
                    loadpresetrock = new RelayCommand((object parameter) => { LoadPreset(25, 10, 5, -10, -5, 5, 10, 20, 20, 20); });
                return loadpresetrock;
            }
        }

        private RelayCommand loadpresettechno;
        public RelayCommand LoadPresetTechno
        {
            get
            {
                if (loadpresettechno == null)
                    loadpresettechno = new RelayCommand((object parameter) => { LoadPreset(20, 20, 0, -10, -5, 0, 10, 40, 40, 40); });
                return loadpresettechno;
            }
        }

        private RelayCommand loadpresetpop;
        public RelayCommand LoadPresetPop
        {
            get
            {
                if (loadpresetpop == null)
                    loadpresetpop = new RelayCommand((object parameter) => { LoadPreset(5, 20, 25, 20, 5, 0, 0, 0, 5, 10); });
                return loadpresetpop;
            }
        }

        private RelayCommand loadpresetsoftbass;
        public RelayCommand LoadPresetSoftBass
        {
            get
            {
                if (loadpresetsoftbass == null)
                    loadpresetsoftbass = new RelayCommand((object parameter) => { LoadPreset(10, 10, 10, 0, -10, -10, 0, 0, 0, 0); });
                return loadpresetsoftbass;
            }
        }

        private RelayCommand loadpresetsoftheights;
        public RelayCommand LoadPresetSoftHeights
        {
            get
            {
                if (loadpresetsoftheights == null)
                    loadpresetsoftheights = new RelayCommand((object parameter) => { LoadPreset(0, 0, -20, -20, -20, -20, -10, 0, 10, 20); });
                return loadpresetsoftheights;
            }
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
