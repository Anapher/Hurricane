using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Hurricane.Settings.Themes;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.ViewModelBase;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
/*
namespace Hurricane.Designer.Data
{
    public class ThemePack2 : PropertyChangedBase, ISaveable, IDisposable, IApplicationBackground, IAppTheme, IAccentColor, IAudioVisualisationContainer
    {
        [XmlIgnore]
        public string Creator { get; set; }

        [XmlIgnore]
        public string Name { get; set; }

        #region Data
        private BitmapImage _previewImage;
        [JsonIgnore, XmlIgnore]
        public BitmapImage PreviewImage
        {
            get { return _previewImage; }
            set
            {
                SetProperty(value, ref _previewImage);
            }
        }

        [JsonIgnore, XmlIgnore]
        public AppThemeData BaseTheme { get; set; }

        [JsonIgnore, XmlIgnore]
        public AccentColorData ColorTheme { get; set; }

        #endregion

        #region ContainInfo
        private bool _containsBaseTheme;
        [XmlIgnore]
        public bool ContainsAppTheme
        {
            get { return _containsBaseTheme; }
            set
            {
                SetProperty(value, ref _containsBaseTheme);
            }
        }

        private bool _containsColorTheme;
        [XmlIgnore]
        public bool ContainsAccentColor
        {
            get { return _containsColorTheme; }
            set
            {
                SetProperty(value, ref _containsColorTheme);
            }
        }

        private bool _containsAudioVisualisation;
        [XmlIgnore]
        public bool ContainsAudioVisualisation
        {
            get { return _containsAudioVisualisation; }
            set
            {
                SetProperty(value, ref _containsAudioVisualisation);
            }
        }

        private bool _containsBackground;
        [XmlIgnore]
        public bool ContainsBackground
        {
            get { return _containsBackground; }
            set
            {
                SetProperty(value, ref _containsBackground);
            }
        }

        #endregion

        [XmlIgnore]
        public string BackgroundName { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [XmlIgnore, JsonIgnore]
        public string AbsolutePath { get; set; }

        public static ThemePack CreateNew()
        {
            return new ThemePack();
        }

        public static ThemePack LoadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var zf = new ZipFile(fs))
            {
                var ze = zf.GetEntry("info.json");
                if (ze == null)
                {
                    throw new Exception("info.json was not found");
                }

                using (var s = zf.GetInputStream(ze))
                using(var reader = new StreamReader(s))
                {
                    var themePack = JsonConvert.DeserializeObject<ThemePack>(reader.ReadToEnd());
                    themePack.AbsolutePath = path;
                    if (themePack.ContainsAccentColor)
                    {
                        using (var colorThemeReader = new StreamReader(zf.GetInputStream(zf.GetEntry(AccentColorName))))
                        {
                            var data = new AccentColorData();
                            data.LoadFromString(colorThemeReader.ReadToEnd());
                        }
                    }

                    if (themePack.ContainsBackground)
                    {
                        themePack._backgroundPath = Path.Combine(Path.GetTempPath(),
                            "HurricaneBackground" + new FileInfo(themePack.AbsolutePath).Extension);
                    }

                    if (themePack.ContainsAppTheme)
                    {
                        using (var baseThemeReader = new StreamReader(zf.GetInputStream(zf.GetEntry(AppThemeName))))
                        {
                            var data = new AppThemeData();
                            data.LoadFromString(baseThemeReader.ReadToEnd());
                        }
                    }

                    if (themePack.ContainsAudioVisualisation)
                    {
                        using (var stream = zf.GetInputStream(zf.GetEntry(AudioVisualisationName)))
                        {
                            themePack._audioVisualisationPlugin =
                                AudioVisualisationPluginHelper.FromStream(stream);
                        }
                    }
                    
                    themePack.FileName = new FileInfo(path).Name;
                    return themePack;
                }
            }
        }

        private static string GetDefaultText()
        {
            return Application.Current.Resources["FromThemePack"].ToString();
        }

        public void Save(string path)
        {
            var folderToCompress = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "HurricaneThemePack"));
            if (folderToCompress.Exists) folderToCompress.Delete(true);

            var jsonFile = new FileInfo(Path.Combine(folderToCompress.FullName, "info.json"));
            JsonConvert.SerializeObject(this);

            using (var fs = File.Open(jsonFile.FullName, FileMode.CreateNew))
            using (var sw = new StreamWriter(fs))
            using (var jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                var serializer = new JsonSerializer();
                serializer.Serialize(jw, this);
            }

            if (ContainsAudioVisualisation)
            {
                
            }

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using(var zipStream = new ZipOutputStream(fs))
            {
                zipStream.SetLevel(3);


                var newEntry = new ZipEntry("");
                

            }


        }

        public void Dispose()
        {
            var fi = new FileInfo(_backgroundPath);
            if (fi.Exists) fi.Delete();
        }

        #region BaseTheme

        string IAppTheme.Name
        {
            get { return GetDefaultText(); }
        }

        string IAppTheme.TranslatedName
        {
            get { return GetDefaultText(); }
        }

        void IAppTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("basetheme", ColorTheme.GetResourceDictionary());
        }

        bool IAppTheme.UseLightDialogs
        {
            get
            {
                return
                    BaseTheme.ThemeSettings.OfType<ThemeBoolean>()
                        .First(x => x.ID == "UseDialogsForWhiteTheme")
                        .BooleanValue;
            }
        }

        #endregion

        #region ColorTheme

        string IAccentColor.Name
        {
            get { return GetDefaultText(); }
        }

        string IAccentColor.TranslatedName
        {
            get { return GetDefaultText(); }
        }

        void IAccentColor.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("colortheme", ColorTheme.GetResourceDictionary());
        }

        #endregion

        #region ApplicationBackground

        private string _backgroundPath;

        Uri IApplicationBackground.GetBackground()
        {
            if (!ContainsBackground) return null;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            using (var zf = new ZipFile(fs))
            {
                var backgroundZipEntry = zf.GetEntry(BackgroundName);
                var zipStream = zf.GetInputStream(backgroundZipEntry);
                var buffer = new byte[4096];
                var file = new FileInfo(_backgroundPath);
                if (file.Exists) file.Delete();
                using (var streamWriter = File.Create(file.FullName))
                {
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                }
                return new Uri(file.FullName);
            }
        }

        bool IApplicationBackground.IsAnimated
        {
            get { return Utilities.GeneralHelper.IsVideo(BackgroundName); }
        }

        bool IApplicationBackground.IsAvailable
        {
            get { return true; }
        }

        string IApplicationBackground.DisplayText
        {
            get { return GetDefaultText(); }
        }

        #endregion

        private IAudioVisualisationPlugin _audioVisualisationPlugin;
        IAudioVisualisationPlugin IAudioVisualisationContainer.AudioVisualisationPlugin
        {
            get { return _audioVisualisationPlugin; }
        }
    }
}*/