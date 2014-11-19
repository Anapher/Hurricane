using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows;

namespace Hurricane.Settings
{
    [Serializable, XmlType(TypeName = "Settings")]
    public class ConfigSettings : SettingsBase, IEquatable<ConfigSettings>
    {
        protected const string Filename = "config.xml";

        //CSCore
        public string SoundOutDeviceID { get; set; }
        public long TrackPosition { get; set; }
        public float Volume { get; set; }

        //Current State
        public int LastPlaylistIndex { get; set; }
        public int LastTrackIndex { get; set; }
        public int SelectedPlaylist { get; set; }
        public int SelectedTrack { get; set; }

        //Playback
        public bool RepeatTrack { get; set; }
        public bool RandomTrack { get; set; }
        public Music.EqualizerSettings EqualizerSettings { get; set; }
        public int WaveSourceBits { get; set; }
        public int SampleRate { get; set; }

        //Magic Arrow
        public bool DisableMagicArrowInGame { get; set; }
        public bool ShowMagicArrowBelowCursor { get; set; }
        public MagicArrow.DockManager.DockingApplicationState ApplicationState { get; set; }

        //General
        public string Language { get; set; }
        public Notification.NotificationType Notification { get; set; }
        public bool DisableNotificationInGame { get; set; }

        private List<LanguageInfo> languages;
        [XmlIgnore]
        public List<LanguageInfo> Languages
        {
            get
            {
                if (languages == null)
                {
                    languages = new List<LanguageInfo>();
                    languages.Add(new LanguageInfo("Deutsch", "/Resources/Languages/Hurricane.de-de.xaml", new Uri("/Resources/Languages/Icons/de.png",UriKind.Relative), "Alkaline", "de"));
                    languages.Add(new LanguageInfo("English", "/Resources/Languages/Hurricane.en-us.xaml", new Uri("/Resources/Languages/Icons/us.png",UriKind.Relative), "Alkaline", "en"));
                }
                return languages;
            }
        }

        public override void SetStandardValues()
        {
            SoundOutDeviceID = "-0";
            LastPlaylistIndex = -1;
            LastTrackIndex = -1;
            TrackPosition = 0;
            Volume = 1.0f;
            SelectedPlaylist = 0;
            SelectedTrack = -1;
            RepeatTrack = false;
            RandomTrack = false;
            EqualizerSettings = new Music.EqualizerSettings();
            EqualizerSettings.CreateNew();
            DisableMagicArrowInGame = true;
            DisableNotificationInGame = true;
            ShowMagicArrowBelowCursor = true;
            WaveSourceBits = 16;
            SampleRate = -1;
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "de") { this.Language = "de"; } else { this.Language = "en"; }
            Notification = Hurricane.Notification.NotificationType.Top;
            ApplicationState = null;
        }

        private ResourceDictionary lastLanguage;
        public void LoadLanguage()
        {
            if (lastLanguage != null) Application.Current.Resources.Remove(lastLanguage);
            LanguageInfo info = new LanguageInfo(Language);
            info.Load(Languages);
            lastLanguage = new ResourceDictionary() { Source = new Uri(info.Path, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(lastLanguage);
        }

        public override void Save(string ProgramPath)
        {
            this.Save<ConfigSettings>(Path.Combine(ProgramPath, Filename));
        }

        public static ConfigSettings Load(string Programpath)
        {
            FileInfo fi = new FileInfo(Path.Combine(Programpath, Filename));
            ConfigSettings result;
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(Path.Combine(Programpath, Filename)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(ConfigSettings));
                    result = (ConfigSettings)deserializer.Deserialize(reader);
                }
            }
            else
            {
                result = new ConfigSettings();
                result.SetStandardValues();
            }
            result.LoadLanguage();
            return result;
        }

        public bool Equals(ConfigSettings other)
        {
            if (other == null) return false;
            return (CompareTwoValues(this.SoundOutDeviceID, other.SoundOutDeviceID) &&
                CompareTwoValues(this.DisableMagicArrowInGame, other.DisableMagicArrowInGame) &&
                CompareTwoValues(this.WaveSourceBits, other.WaveSourceBits) &&
                CompareTwoValues(this.ShowMagicArrowBelowCursor, other.ShowMagicArrowBelowCursor) &&
                CompareTwoValues(this.SampleRate, other.SampleRate) &&
                CompareTwoValues(this.Language, other.Language) &&
                CompareTwoValues(this.Notification, other.Notification) &&
                CompareTwoValues(this.DisableNotificationInGame, other.DisableNotificationInGame));
        }

        protected bool CompareTwoValues(object v1, object v2)
        {
            if (v1 == null || v2 == null) return false;
            return v1.Equals(v2);
        }
    }
}
