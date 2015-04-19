using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Serialization;
using Hurricane.PluginAPI.AudioVisualisation;
using Hurricane.Settings.Themes;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.Utilities;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Hurricane.Designer.Data
{
    public class ThemePack : IApplicationBackground, IAppTheme, IAccentColor, IAudioVisualisationContainer
    {
        [XmlIgnore]
        public string Creator { get; set; }

        [XmlIgnore]
        public string Name { get; set; }

        public string FileName { get; set; }

        #region ContainInfo

        [XmlIgnore]
        public bool ContainsAppTheme { get; set; }

        [XmlIgnore]
        public bool ContainsAccentColor { get; set; }

        [XmlIgnore]
        public bool ContainsAudioVisualisation { get; set; }

        [XmlIgnore]
        public bool ContainsBackground { get; set; }

        #endregion

        [XmlIgnore]
        public string BackgroundName { get; set; }


        public static bool FromFile(string fileName, out ThemePack result)
        {
            var fiSource = new FileInfo(fileName);

            using (var fs = new FileStream(fiSource.FullName, FileMode.Open, FileAccess.Read))
            using (var zf = new ZipFile(fs))
            {
                var ze = zf.GetEntry("info.json");
                if (ze == null)
                {
                    result = null;
                    return false;
                }

                using (var s = zf.GetInputStream(ze))
                using (var reader = new StreamReader(s))
                {
                    var themePack = JsonConvert.DeserializeObject<ThemePack>(reader.ReadToEnd());
                    themePack.FileName = fiSource.Name;

                    result = themePack;
                    return true;
                }
            }
        }

        public async Task Load(string filePath)
        {
            var fiSource = new FileInfo(filePath);

            using (var fs = new FileStream(fiSource.FullName, FileMode.Open, FileAccess.Read))
            using (var zf = new ZipFile(fs))
            {
                if (ContainsAudioVisualisation)
                {
                    using (var stream = zf.GetInputStream(zf.GetEntry(ThemePackConsts.AudioVisualisationName)))
                    {
                        _audioVisualisationPlugin = await Task.Run(() => AudioVisualisationPluginHelper.FromStream(stream));
                    }
                }

                if (ContainsBackground)
                {
                    var path = "HurricaneBackground" + BackgroundName;
                    var backgroundZipEntry = zf.GetEntry(BackgroundName);
                    using (var zipStream = zf.GetInputStream(backgroundZipEntry))
                    {
                        var buffer = new byte[4096];
                        var file = new FileInfo(path);
                        if (file.Exists) file.Delete();
                        using (var streamWriter = File.Create(file.FullName))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                        _backgroundPath = file.FullName;
                    }
                }

                if (ContainsAppTheme)
                {
                    using (var stream = zf.GetInputStream(zf.GetEntry(ThemePackConsts.AppThemeName)))
                    {
                        _appThemeResourceDictionary = (ResourceDictionary)XamlReader.Load(stream);
                    }
                } 
                
                if (ContainsAccentColor)
                {
                    using (var stream = zf.GetInputStream(zf.GetEntry(ThemePackConsts.AccentColorName)))
                    {
                        _accentColorResourceDictionary = (ResourceDictionary)XamlReader.Load(stream);
                    }
                }
            }
        }

        public void Unload()
        {
            if (!string.IsNullOrEmpty(_backgroundPath))
            {
                var fiBackground = new FileInfo(_backgroundPath);
                if (fiBackground.Exists) fiBackground.Delete();
            }
        }

        #region IAudioVisualisationContainer

        private IAudioVisualisationPlugin _audioVisualisationPlugin;
        IAudioVisualisationPlugin IAudioVisualisationContainer.Visualisation
        {
            get { return _audioVisualisationPlugin; }
        }

        string IAudioVisualisationContainer.Name
        {
            get { return DefaultText; }
        }

        #endregion

        #region IAccentColor

        private ResourceDictionary _accentColorResourceDictionary;

        string IAccentColor.Name
        {
            get { return DefaultText; }
        }

        string IAccentColor.TranslatedName
        {
            get { return DefaultText; }
        }

        void IAccentColor.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("accentcolor", _accentColorResourceDictionary);
        }

        ResourceDictionary IAccentColor.ResourceDictionary
        {
            get { return _accentColorResourceDictionary; }
        }

        #endregion

        #region IAppTheme

        private ResourceDictionary _appThemeResourceDictionary;

        string IAppTheme.Name
        {
            get { return DefaultText; }
        }

        string IAppTheme.TranslatedName
        {
            get { return DefaultText; }
        }

        void IAppTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("apptheme", _appThemeResourceDictionary);
        }

        ResourceDictionary IAppTheme.ResourceDictionary
        {
            get { return _appThemeResourceDictionary; }
        }

        #endregion

        #region IApplicationBackground

        private string _backgroundPath;
        Uri IApplicationBackground.GetBackground()
        {
            return new Uri(_backgroundPath, UriKind.Absolute);
        }

        bool IApplicationBackground.IsAnimated
        {
            get { return FileSystemHelper.IsVideo(BackgroundName); }
        }

        bool IApplicationBackground.IsAvailable
        {
            get { return true; }
        }

        string IApplicationBackground.DisplayText
        {
            get { return DefaultText; }
        }

        #endregion

        public string DefaultText
        {
            get
            {
                return Application.Current.Resources["FromThemePack"].ToString();
            }
        }
    }
}