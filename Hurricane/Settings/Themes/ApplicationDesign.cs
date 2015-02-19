using System;
using System.Linq;
using System.Xml.Serialization;
using Hurricane.Designer.Data;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.AudioVisualisation.DefaultAudioVisualisation;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.Settings.Themes.Visual.BaseThemes;
using Hurricane.Settings.Themes.Visual.ColorThemes;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class ApplicationDesign : PropertyChangedBase
    {
        [XmlIgnore]
        public IColorTheme ColorTheme { get; set; }

        [XmlIgnore]
        public IBaseTheme BaseTheme { get; set; }

        private IApplicationBackground _applicationBackground;
        [XmlIgnore]
        public IApplicationBackground ApplicationBackground
        {
            get { return _applicationBackground; }
            set
            {
                SetProperty(value, ref _applicationBackground);
            }
        }
        
        private IAudioVisualisationContainer _audioVisualisation;
        [XmlIgnore]
        public IAudioVisualisationContainer AudioVisualisation
        {
            get { return _audioVisualisation; }
            set
            {
                var oldValue = _audioVisualisation;
                if (SetProperty(value, ref _audioVisualisation) && oldValue != null)
                {
                    oldValue.AudioVisualisationPlugin.AdvancedWindowVisualisation.Dispose();
                    oldValue.AudioVisualisationPlugin.SmartWindowVisualisation.Dispose();
                }
            }
        }

        public void SetStandard()
        {
            var themeManager = ApplicationThemeManager.Instance;
            ColorTheme = themeManager.ColorThemes.First(x => x.Name == "Blue");
            BaseTheme = themeManager.BaseThemes.First(x => x.Name == "BaseLight");
            ApplicationBackground = null;
            AudioVisualisation = DefaultAudioVisualisation.GetDefault();
        }

        #region Workaround for serializing interfaces

        [XmlElement("ColorTheme", Type = typeof(ColorThemeBase))]
        [XmlElement("ColorThemePack", Type = typeof(ThemePack))]
        public object SerializableColorTheme
        {
            get
            {
                return ColorTheme; 
            }
            set
            {
                var colorTheme = (IColorTheme) value;
                if (colorTheme is ColorThemeBase)
                {
                    ColorTheme = ApplicationThemeManager.Instance.ColorThemes.FirstOrDefault(x => x.Equals(colorTheme));
                }
                else if(colorTheme is ThemePack)
                {
                    ColorTheme = ApplicationThemeManager.Instance.GetThemePack(((ThemePack)value).FileName);
                }
            }
        }

        [XmlElement("BaseTheme", Type = typeof(BaseThemeBase))]
        [XmlElement("BaseThemePack", Type = typeof(ThemePack))]
        public object SerializableBaseTheme
        {
            get { return BaseTheme; }
            set
            {
                var baseTheme = (IBaseTheme)value;
                if (baseTheme is BaseThemeBase)
                {
                    BaseTheme = ApplicationThemeManager.Instance.BaseThemes.FirstOrDefault(x => x.Equals(baseTheme));
                }
                else if (baseTheme is ThemePack)
                {
                    ColorTheme = ApplicationThemeManager.Instance.GetThemePack(((ThemePack)value).FileName);
                }
            }
        }

        [XmlElement("ApplicationBackground", Type = typeof(CustomApplicationBackground))]
        [XmlElement("ApplicationBackgroundPack", Type = typeof(ThemePack))]
        public object SerializableBackgroundImage
        {
            get { return ApplicationBackground; }
            set
            {
                if (value is ThemePack)
                {
                    ApplicationBackground = ApplicationThemeManager.Instance.GetThemePack(((ThemePack) value).FileName);
                }
                else
                {
                    ApplicationBackground = (IApplicationBackground)value;
                }
            }
        }

        [XmlElement("AudioVisualisation", Type = typeof(DefaultAudioVisualisation))]
        [XmlElement("AudioVisualisationPack", Type = typeof(ThemePack))]
        [XmlElement("CustomAudioVisualisation", Type = typeof(CustomAudioVisualisation))]
        public object SerializableAudioVisualisation
        {
            get { return AudioVisualisation; }
            set
            {
                if (value is DefaultAudioVisualisation)
                {
                    AudioVisualisation = DefaultAudioVisualisation.GetDefault();
                }
                else if (value is CustomAudioVisualisation)
                {
                    var visualisation = (CustomAudioVisualisation) value;

                    AudioVisualisation =
                        ApplicationThemeManager.Instance.AudioVisualisations.OfType<CustomAudioVisualisation>().FirstOrDefault(
                            x => x.FileName == visualisation.FileName);
                    if (AudioVisualisation == null) AudioVisualisation = DefaultAudioVisualisation.GetDefault();
                }
                else if (value is ThemePack)
                {
                    AudioVisualisation = ApplicationThemeManager.Instance.GetThemePack(((ThemePack) value).FileName);
                }
            }
        }

        #endregion

        public bool Equals(ApplicationDesign obj)
        {
            return ColorTheme.Equals(obj.ColorTheme) && BaseTheme.Equals(obj.BaseTheme) &&
                ApplicationBackground != null && ApplicationBackground.Equals(obj.ApplicationBackground);
        }
    }
}