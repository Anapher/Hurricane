using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using AudioVisualisation;
using Hurricane.Settings.Themes;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.ViewModelBase;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Hurricane.Designer.Data
{
    public class ThemePack : PropertyChangedBase, ISaveable, IDisposable, IBackgroundImage, IBaseTheme, IColorTheme, IAudioVisualisationContainer
    {
        private const string ColorThemeName = "ColorTheme.xaml";
        private const string BaseThemeName = "BaseTheme.xaml";
        private const string SpectrumAnalyzerName = "SpectrumAnalyzer.dll";

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
        public BaseThemeData BaseTheme { get; set; }

        [JsonIgnore, XmlIgnore]
        public ColorThemeData ColorTheme { get; set; }

        [JsonIgnore, XmlIgnore]
        public UIElement SpectrumAnalyzer { get; set; }

        [JsonIgnore, XmlIgnore]
        public BitmapImage BackgroundImage { get; set; }

        #endregion

        #region ContainInfo
        private bool _containsBaseTheme;
        [XmlIgnore]
        public bool ContainsBaseTheme
        {
            get { return _containsBaseTheme; }
            set
            {
                SetProperty(value, ref _containsBaseTheme);
            }
        }

        private bool _containsColorTheme;
        [XmlIgnore]
        public bool ContainsColorTheme
        {
            get { return _containsColorTheme; }
            set
            {
                SetProperty(value, ref _containsColorTheme);
            }
        }

        private bool _containsSpectrumAnalyzer;
        [XmlIgnore]
        public bool ContainsSpectrumAnalyzer
        {
            get { return _containsSpectrumAnalyzer; }
            set
            {
                SetProperty(value, ref _containsSpectrumAnalyzer);
            }
        }

        private bool _containsBackgroundImage;
        [XmlIgnore]
        public bool ContainsBackgroundImage
        {
            get { return _containsBackgroundImage; }
            set
            {
                SetProperty(value, ref _containsBackgroundImage);
            }
        }

        private bool _containsWindowSkin;
        [XmlIgnore]
        public bool ContainsWindowSkin
        {
            get { return _containsWindowSkin; }
            set
            {
                SetProperty(value, ref _containsWindowSkin);
            }
        }

        #endregion

        [XmlIgnore]
        public string BackgroundName { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

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

                    if (themePack.ContainsBackgroundImage)
                    {
                        var backgroundZipEntry = zf.GetEntry(themePack.BackgroundName);
                        themePack.BackgroundImage =
                            Utilities.ImageHelper.StreamToBitmapImage(zf.GetInputStream(backgroundZipEntry));
                    }

                    if (themePack.ContainsColorTheme)
                    {
                        using (var colorThemeReader = new StreamReader(zf.GetInputStream(zf.GetEntry(ColorThemeName))))
                        {
                            var data = new ColorThemeData();
                            data.LoadFromString(colorThemeReader.ReadToEnd());
                        }
                    }

                    if (themePack.ContainsBaseTheme)
                    {
                        using (var baseThemeReader = new StreamReader(zf.GetInputStream(zf.GetEntry(BaseThemeName))))
                        {
                            var data = new BaseThemeData();
                            data.LoadFromString(baseThemeReader.ReadToEnd());
                        }
                    }

                    if (themePack.ContainsSpectrumAnalyzer)
                    {
                        using (var stream = zf.GetInputStream(zf.GetEntry(SpectrumAnalyzerName)))
                        {
                            themePack._spectrumAnalyzerPlugin =
                                Settings.Themes.AudioVisualisation.AudioVisualisationPluginHelper.FromStream(stream);
                        }
                    }

                    themePack.FileName = path;
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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if(BackgroundImage != null) BackgroundImage.StreamSource.Dispose();
        }

        #region BaseTheme

        string IBaseTheme.Name
        {
            get { return GetDefaultText(); }
        }

        string IBaseTheme.TranslatedName
        {
            get { return GetDefaultText(); }
        }

        void IBaseTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("basetheme", ColorTheme.GetResourceDictionary());
        }

        bool IBaseTheme.UseLightDialogs
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

        string IColorTheme.Name
        {
            get { return GetDefaultText(); }
        }

        string IColorTheme.TranslatedName
        {
            get { return GetDefaultText(); }
        }

        void IColorTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("colortheme", ColorTheme.GetResourceDictionary());
        }

        #endregion

        #region BackgroundImage

        BitmapImage IBackgroundImage.GetBackgroundImage()
        {
            return BackgroundImage;
        }

        bool IBackgroundImage.IsAnimated
        {
            get { return BackgroundName.EndsWith(".gif"); }
        }

        bool IBackgroundImage.IsAvailable
        {
            get { return true; }
        }

        string IBackgroundImage.DisplayText
        {
            get { return GetDefaultText(); }
        }

        #endregion

        private IAudioVisualisationPlugin _spectrumAnalyzerPlugin;
        IAudioVisualisationPlugin IAudioVisualisationContainer.AudioVisualisationPlugin
        {
            get { return _spectrumAnalyzerPlugin;; }
        }
    }
}