using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using Hurricane.AppCommunication;
using Hurricane.Music.AudioEngine;
using Hurricane.Music.Download;
using Hurricane.Notification;
using Hurricane.Settings.Themes;
using MahApps.Metro.Controls;
// ReSharper disable InconsistentNaming

namespace Hurricane.Settings
{
    [Serializable, XmlType(TypeName = "Settings")]
    public class ConfigSettings : SettingsBase
    {
        private const string Filename = "config.xml";

        //CSCore
        public string SoundOutDeviceID { get; set; }
        public SoundOutMode SoundOutMode { get; set; }
        public int Latency { get; set; }
        public bool IsCrossfadeEnabled { get; set; }
        public int CrossfadeDuration { get; set; }

        //Playback
        public int WaveSourceBits { get; set; }
        public int SampleRate { get; set; }

        //Magic Arrow
        public bool ShowMagicArrowBelowCursor { get; set; }

        //Design
        public ApplicationDesign ApplicationDesign { get; set; }

        private bool _useThinHeaders;
        public bool UseThinHeaders
        {
            get { return _useThinHeaders; }
            set
            {
                SetProperty(value, ref _useThinHeaders);
            }
        }
        
        private TransitionType _tabControlTransition;
        public TransitionType TabControlTransition
        {
            get { return _tabControlTransition; }
            set
            {
                SetProperty(value, ref _tabControlTransition);
            }
        }

        //General
        public string Language { get; set; }
        public bool RememberTrackImportPlaylist { get; set; }
        public string PlaylistToImportTrack { get; set; }
        public bool ShufflePreferFavoriteTracks { get; set; }
        public bool ShowArtistAndTitle { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ShowNotificationIfMinimizeToTray { get; set; }
        public bool ShowProgressInTaskbar { get; set; }

        //Notifications
        public NotificationType Notification { get; set; }
        public bool DisableNotificationInGame { get; set; }
        public int NotificationShowTime { get; set; }

        //Music
        public bool LoadAlbumCoverFromInternet { get; set; }
        public ImageQuality DownloadAlbumCoverQuality { get; set; }
        public bool SaveCoverLocal { get; set; }
        public bool TrimTrackname { get; set; }
        public DownloadManager Downloader { get; set; }
        public ConverterSettings ConverterSettings { get; set; }

        //App
        public AppCommunicationSettings AppCommunicationSettings { get; set; }
        [XmlIgnore]
        public AppCommunicationManager AppCommunicationManager { get; set; }

        private List<LanguageInfo> _languages;
        [XmlIgnore]
        public List<LanguageInfo> Languages
        {
            get
            {
                //Language codes: http://www.lingoes.net/en/translator/langcode.htm
                return _languages ?? (_languages = new List<LanguageInfo>
                {
                    new LanguageInfo("Deutsch", "/Resources/Languages/Hurricane.de-de.xaml",
                        new Uri("/Resources/Languages/Icons/de.png", UriKind.Relative), "Alkaline", "de"),
                    new LanguageInfo("English", "/Resources/Languages/Hurricane.en-us.xaml",
                        new Uri("/Resources/Languages/Icons/us.png", UriKind.Relative), "Alkaline", "en"),
                    new LanguageInfo("Nederlands", "/Resources/Languages/Hurricane.nl-nl.xaml",
                        new Uri("/Resources/Languages/Icons/nl.png", UriKind.Relative), "DrawCase", "nl"),
                    new LanguageInfo("Suomi", "/Resources/Languages/Hurricane.fi-fi.xaml",
                        new Uri("/Resources/Languages/Icons/fi.png", UriKind.Relative), "Väinämö Vettenranta", "fi")
                });
            }
        }

        public override sealed void SetStandardValues()
        {
            SoundOutDeviceID = SoundOutManager.DefaultDevicePlaceholder;
            DisableNotificationInGame = true;
            ShowMagicArrowBelowCursor = true;
            WaveSourceBits = 16;
            SampleRate = -1;
            var language = Languages.FirstOrDefault(x => x.Code == Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
            Language = language == null ? "en" : language.Code;
            Notification = NotificationType.Top;
            ApplicationDesign = new ApplicationDesign();
            ApplicationDesign.SetStandard();
            NotificationShowTime = 5000;
            RememberTrackImportPlaylist = false;
            PlaylistToImportTrack = null;
            LoadAlbumCoverFromInternet = true;
            DownloadAlbumCoverQuality = ImageQuality.Maximum;
            SaveCoverLocal = false;
            TrimTrackname = true;
            ShowArtistAndTitle = true;
            SoundOutMode = CSCore.SoundOut.WasapiOut.IsSupportedOnCurrentPlatform ? SoundOutMode.WASAPI : SoundOutMode.DirectSound;
            Latency = 100;
            IsCrossfadeEnabled = false;
            CrossfadeDuration = 4;
            UseThinHeaders = true;
            MinimizeToTray = false;
            ShowNotificationIfMinimizeToTray = true;
            Downloader = new DownloadManager();
            TabControlTransition = TransitionType.Left;
            ShowProgressInTaskbar = true;
            AppCommunicationSettings = new AppCommunicationSettings();
            AppCommunicationSettings.SetStandard();
            ConverterSettings = new ConverterSettings();
            ConverterSettings.SetDefault();
        }

        public ConfigSettings()
        {
            SetStandardValues();
        }

        public void LoadAppCommunication()
        {
            if (AppCommunicationManager == null)
                AppCommunicationManager = new AppCommunicationManager(AppCommunicationSettings);
            if (AppCommunicationSettings.IsEnabled) AppCommunicationManager.Start();
        }

        private ResourceDictionary _lastLanguage;
        public void LoadLanguage()
        {
            if (_lastLanguage != null) Application.Current.Resources.MergedDictionaries.Remove(_lastLanguage);
            _lastLanguage = new ResourceDictionary { Source = new Uri(Languages.First(x => x.Code == Language).Path, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(_lastLanguage);
        }

        public override void Save(string programPath)
        {
            Save<ConfigSettings>(Path.Combine(programPath, Filename));
        }

        public static ConfigSettings Load(string programpath)
        {
            var fi = new FileInfo(Path.Combine(programpath, Filename));
            ConfigSettings result;
            if (!fi.Exists || string.IsNullOrWhiteSpace(File.ReadAllText(fi.FullName)))
            {
                result = new ConfigSettings();
            }
            else
            {
                using (var reader = new StreamReader(Path.Combine(programpath, Filename)))
                {
                    var deserializer = new XmlSerializer(typeof(ConfigSettings));
                    result = (ConfigSettings)deserializer.Deserialize(reader);
                }
            }
            result.LoadLanguage();
            result.LoadAppCommunication();
            return result;
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
