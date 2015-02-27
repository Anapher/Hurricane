using System.IO;
using System.Xml.Serialization;
using Hurricane.Designer.Data.ThemeData;
using Hurricane.ViewModelBase;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Hurricane.Designer.Data
{
    public class EditableThemePack : PropertyChangedBase
    {
        public string ThemePackPath { get; set; }
        public string Creator { get; set; }
        public string Name { get; set; }
        public string BackgroundPath { get; set; }
        public string AudioVisualisationPath { get; set; }

        private AppThemeData _appTheme;
        public AppThemeData AppTheme
        {
            get { return _appTheme; }
            set
            {
                SetProperty(value, ref _appTheme);
            }
        }

        private AccentColorData _accentColor;
        public AccentColorData AccentColor
        {
            get { return _accentColor; }
            set
            {
                SetProperty(value, ref _accentColor);
            }
        }

        #region ContainInfo
        private bool _containsAppTheme;
        [XmlIgnore]
        public bool ContainsAppTheme
        {
            get { return _containsAppTheme; }
            set
            {
                SetProperty(value, ref _containsAppTheme);
            }
        }

        private bool _containsAccentColor;
        [XmlIgnore]
        public bool ContainsAccentColor
        {
            get { return _containsAccentColor; }
            set
            {
                SetProperty(value, ref _containsAccentColor);
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

        public void Save()
        {
            var themePack = new ThemePack
            {
                ContainsAudioVisualisation = ContainsAudioVisualisation,
                ContainsBackground = ContainsBackground,
                ContainsAppTheme = ContainsAppTheme,
                ContainsAccentColor = ContainsAccentColor
            };

            var folderToCompress = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "HurricaneThemePack"));
            if (folderToCompress.Exists) folderToCompress.Delete(true);

            using (var fs = new FileStream(ThemePackPath, FileMode.Create, FileAccess.Write))
            using (var zipStream = new ZipOutputStream(fs))
            {
                zipStream.SetLevel(3);
                if (ContainsAudioVisualisation)
                {

                }


                var jsonFile = new FileInfo(Path.Combine(folderToCompress.FullName, "info.json"));

                using (var jsonStream = File.Open(jsonFile.FullName, FileMode.CreateNew))
                using (var sw = new StreamWriter(jsonStream))
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;

                    var serializer = new JsonSerializer();
                    serializer.Serialize(jw, this);
                }




                var newEntry = new ZipEntry("");


            }
        }
    }
}