using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using Hurricane.MagicArrow.DockManager;
using Hurricane.Music;
using Hurricane.Notification;
using Hurricane.Settings.Themes;

namespace Hurricane.Settings
{
    [Serializable, XmlType(TypeName = "Settings")]
    public class ConfigSettings : SettingsBase, IEquatable<ConfigSettings>
    {
        protected const string Filename = "config.xml";

        //CSCore
        public string SoundOutDeviceID { get; set; }
        public SoundOutMode SoundOutMode { get; set; }
        public float Volume { get; set; }
        public int Latency { get; set; }
        public bool IsCrossfadeEnabled { get; set; }
        public int CrossfadeDuration { get; set; }

        //Current State
        public long TrackPosition { get; set; }
        public int LastPlaylistIndex { get; set; }
        public int LastTrackIndex { get; set; }
        public int SelectedPlaylist { get; set; }
        public int SelectedTrack { get; set; }
        public QueueManager Queue { get; set; }

        //Playback
        public bool IsLoopEnabled { get; set; }
        public bool IsShuffleEnabled { get; set; }
        public EqualizerSettings EqualizerSettings { get; set; }
        public int WaveSourceBits { get; set; }
        public int SampleRate { get; set; }

        //Magic Arrow
        public bool ShowMagicArrowBelowCursor { get; set; }
        public DockingApplicationState ApplicationState { get; set; }

        //General
        public string Language { get; set; }
        public ApplicationThemeManager Theme { get; set; }
        public bool EnableAdvancedView { get; set; }
        public bool RememberTrackImportPlaylist { get; set; }
        public string PlaylistToImportTrack { get; set; }
        public bool ShufflePreferFavoritTracks { get; set; }
        public bool ShowArtistAndTitle { get; set; }

        public bool ApiIsEnabled { get; set; }
        public int ApiPort { get; set; }

        //Notifications
        public NotificationType Notification { get; set; }
        public bool DisableNotificationInGame { get; set; }
        public int NotificationShowTime { get; set; }

        //Album Cover
        public bool LoadAlbumCoverFromInternet { get; set; }
        public ImageQuality DownloadAlbumCoverQuality { get; set; }
        public bool SaveCoverLocal { get; set; }
        public bool TrimTrackname { get; set; }

        private List<LanguageInfo> _languages;
        [XmlIgnore]
        public List<LanguageInfo> Languages
        {
            get
            {
                return _languages ?? (_languages = new List<LanguageInfo>
                {
                    new LanguageInfo("Deutsch", "/Resources/Languages/Hurricane.de-de.xaml",
                        new Uri("/Resources/Languages/Icons/de.png", UriKind.Relative), "Alkaline", "de"),
                    new LanguageInfo("English", "/Resources/Languages/Hurricane.en-us.xaml",
                        new Uri("/Resources/Languages/Icons/us.png", UriKind.Relative), "Alkaline", "en"),
                                            new LanguageInfo("Suomi", "/Resources/Languages/Hurricane.fi-fi.xaml",
                        new Uri("/Resources/Languages/Icons/fi.png", UriKind.Relative), "Väinämö Vettenranta", "fi")
                });
            }
        }

        public override sealed void SetStandardValues()
        {
            SoundOutDeviceID = "-0";
            LastPlaylistIndex = -10;
            LastTrackIndex = -1;
            TrackPosition = 0;
            Volume = 1.0f;
            SelectedPlaylist = 0;
            SelectedTrack = -1;
            IsLoopEnabled = false;
            IsShuffleEnabled = false;
            EqualizerSettings = new EqualizerSettings();
            EqualizerSettings.CreateNew();
            DisableNotificationInGame = true;
            ShowMagicArrowBelowCursor = true;
            WaveSourceBits = 16;
            SampleRate = -1;
            var language = Languages.FirstOrDefault(x => x.Code == Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
            Language = language == null ? "en" : language.Code;
            Notification = NotificationType.Top;
            ApplicationState = null;
            Theme = new ApplicationThemeManager();
            Theme.LoadStandard();
            EnableAdvancedView = true;
            NotificationShowTime = 5000;
            RememberTrackImportPlaylist = false;
            PlaylistToImportTrack = null;
            LoadAlbumCoverFromInternet = true;
            DownloadAlbumCoverQuality = ImageQuality.Maximum;
            SaveCoverLocal = false;
            TrimTrackname = true;
            ApiIsEnabled = false;
            ApiPort = 10898; //10.08.1998
            ShowArtistAndTitle = true;
            SoundOutMode = CSCore.SoundOut.WasapiOut.IsSupportedOnCurrentPlatform ? SoundOutMode.WASAPI : SoundOutMode.DirectSound;
            Latency = 100;
            IsCrossfadeEnabled = false;
            CrossfadeDuration = 6;
        }

        public ConfigSettings()
        {
            SetStandardValues();
        }

        private ResourceDictionary _lastLanguage;
        public void LoadLanguage()
        {
            if (_lastLanguage != null) Application.Current.Resources.Remove(_lastLanguage);
            _lastLanguage = new ResourceDictionary() { Source = new Uri(this.Languages.First(x => x.Code == Language).Path, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(_lastLanguage);
        }

        public override void Save(string programPath)
        {
            this.Save<ConfigSettings>(Path.Combine(programPath, Filename));
        }

        public static ConfigSettings Load(string programpath)
        {
            FileInfo fi = new FileInfo(Path.Combine(programpath, Filename));
            ConfigSettings result;
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(Path.Combine(programpath, Filename)))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(ConfigSettings));
                    result = (ConfigSettings)deserializer.Deserialize(reader);
                }
            }
            else
            {
                result = new ConfigSettings();
            }
            result.LoadLanguage();
            result.Theme.LoadTheme();
            result.Theme.LoadBaseTheme();
            return result;
        }

        public bool Equals(ConfigSettings other)
        {
            if (other == null) return false;
            return (CompareTwoValues(this.SoundOutDeviceID, other.SoundOutDeviceID) &&
                    CompareTwoValues(this.WaveSourceBits, other.WaveSourceBits) &&
                    CompareTwoValues(this.ShowMagicArrowBelowCursor, other.ShowMagicArrowBelowCursor) &&
                    CompareTwoValues(this.SampleRate, other.SampleRate) &&
                    CompareTwoValues(this.Language, other.Language) &&
                    CompareTwoValues(this.Notification, other.Notification) &&
                    CompareTwoValues(this.DisableNotificationInGame, other.DisableNotificationInGame) &&
                    CompareTwoValues(this.Theme, other.Theme) &&
                    CompareTwoValues(this.EnableAdvancedView, other.EnableAdvancedView) &&
                    CompareTwoValues(this.NotificationShowTime, other.NotificationShowTime) &&
                    CompareTwoValues(this.RememberTrackImportPlaylist, other.RememberTrackImportPlaylist) &&
                    CompareTwoValues(this.DownloadAlbumCoverQuality, other.DownloadAlbumCoverQuality) &&
                    CompareTwoValues(this.LoadAlbumCoverFromInternet, other.LoadAlbumCoverFromInternet) &&
                    CompareTwoValues(this.SaveCoverLocal, other.SaveCoverLocal) &&
                    CompareTwoValues(this.TrimTrackname, other.TrimTrackname) &&
                    CompareTwoValues(this.ApiIsEnabled, other.ApiIsEnabled) &&
                    CompareTwoValues(this.ApiPort, other.ApiPort) &&
                    CompareTwoValues(this.ShufflePreferFavoritTracks, other.ShufflePreferFavoritTracks) &&
                    CompareTwoValues(this.ShowArtistAndTitle, other.ShowArtistAndTitle) &&
                    CompareTwoValues(this.SoundOutMode, other.SoundOutMode) &&
                    CompareTwoValues(this.Latency, other.Latency) &&
                    CompareTwoValues(this.CrossfadeDuration, other.CrossfadeDuration) &&
                    CompareTwoValues(this.IsCrossfadeEnabled, other.IsCrossfadeEnabled));
        }

        protected bool CompareTwoValues(object v1, object v2)
        {
            if (v1 == null || v2 == null) return false;
            return v1.Equals(v2);
        }
    }

    public enum ImageQuality
    {
        Small, Medium, Large, Maximum
    }

    public enum SoundOutMode
    {
        DirectSound, WASAPI
    }
}
