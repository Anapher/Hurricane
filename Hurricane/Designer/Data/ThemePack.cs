using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Serialization;
using AudioVisualisation;
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
    public class ThemePack : IApplicationBackground, IBaseTheme, IColorTheme, IAudioVisualisationContainer
    {
        [XmlIgnore]
        public string Creator { get; set; }

        [XmlIgnore]
        public string Name { get; set; }

        public string FileName { get; set; }

        #region ContainInfo

        [XmlIgnore]
        public bool ContainsBaseTheme { get; set; }

        [XmlIgnore]
        public bool ContainsColorTheme { get; set; }

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

                if (ContainsBaseTheme)
                {
                    using (var stream = zf.GetInputStream(zf.GetEntry(ThemePackConsts.BaseThemeName)))
                    {
                        _baseThemeResourceDictionary = (ResourceDictionary)XamlReader.Load(stream);
                    }
                } 
                
                if (ContainsColorTheme)
                {
                    using (var stream = zf.GetInputStream(zf.GetEntry(ThemePackConsts.ColorThemeName)))
                    {
                        _colorThemeResourceDictionary = (ResourceDictionary)XamlReader.Load(stream);
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
        IAudioVisualisationPlugin IAudioVisualisationContainer.AudioVisualisationPlugin
        {
            get { return _audioVisualisationPlugin; }
        }

        string IAudioVisualisationContainer.Name
        {
            get { return DefaultText; }
        }

        #endregion

        #region IColorTheme

        private ResourceDictionary _colorThemeResourceDictionary;

        string IColorTheme.Name
        {
            get { return DefaultText; }
        }

        string IColorTheme.TranslatedName
        {
            get { return DefaultText; }
        }

        void IColorTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("colortheme", _colorThemeResourceDictionary);
        }

        #endregion

        #region IBaseTheme

        private ResourceDictionary _baseThemeResourceDictionary;

        string IBaseTheme.Name
        {
            get { return DefaultText; }
        }

        string IBaseTheme.TranslatedName
        {
            get { return DefaultText; }
        }

        void IBaseTheme.ApplyTheme()
        {
            ApplicationThemeManager.Instance.LoadResource("basetheme", _baseThemeResourceDictionary);
        }

        bool IBaseTheme.UseLightDialogs
        {
            get { return (bool) _baseThemeResourceDictionary["UseDialogsForWhiteTheme"]; }
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
            get { return GeneralHelper.IsVideo(BackgroundName); }
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