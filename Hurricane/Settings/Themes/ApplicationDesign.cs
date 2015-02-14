using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using AudioVisualisation;
using Hurricane.Designer.Data;
using Hurricane.Settings.Themes.AudioVisualisation;
using Hurricane.Settings.Themes.Background;
using Hurricane.Settings.Themes.Visual;
using Hurricane.Settings.Themes.Visual.BaseThemes;
using Hurricane.Settings.Themes.Visual.ColorThemes;

namespace Hurricane.Settings.Themes
{
    [Serializable]
    public class ApplicationDesign
    {
        [XmlIgnore]
        public IColorTheme ColorTheme { get; set; }

        [XmlIgnore]
        public IBaseTheme BaseTheme { get; set; }

        [XmlIgnore]
        public IBackgroundImage BackgroundImage { get; set; }

        [XmlIgnore]
        public IAudioVisualisationContainer SpectrumAnalyzer { get; set; }

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
                    ColorTheme = ApplicationThemeManager.Instance.ColorThemes.First(x => x.Equals(colorTheme));
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
                    BaseTheme = ApplicationThemeManager.Instance.BaseThemes.First(x => x.Equals(baseTheme));
                }
                else if (baseTheme is ThemePack)
                {
                    ColorTheme = ApplicationThemeManager.Instance.GetThemePack(((ThemePack)value).FileName);
                }
            }
        }

        [XmlElement("BackgroundImage", Type = typeof(CustomBackground))]
        [XmlElement("BackgroundImagePack", Type = typeof(ThemePack))]
        public object SerializableBackgroundImage
        {
            get { return BackgroundImage; }
            set
            {
                if (value is ThemePack)
                {
                    BackgroundImage = ApplicationThemeManager.Instance.GetThemePack(((ThemePack) value).FileName);
                }
                else
                {
                    BackgroundImage = (IBackgroundImage)value;
                }
            }
        }

        private object _spectrumAnalyzerPath;
        [XmlElement("SpectrumAnalyzer", Type = typeof(DefaultAudioVisualisation))]
        [XmlElement("SpectrumAnalyzerPack", Type = typeof(ThemePack))]
        [XmlElement("CustomSpectrumAnalyzer", Type = typeof(CustomAudioVisualisation))]
        public object SerializableSpectrumAnalyzer
        {
            get { return SpectrumAnalyzer; }
            set
            {
                if (value is DefaultAudioVisualisation)
                {
                    SpectrumAnalyzer = DefaultAudioVisualisation.GetDefault();
                }
                else if (value is CustomAudioVisualisation)
                {
                    SpectrumAnalyzer = (CustomAudioVisualisation) value;
                }
                else if (value is ThemePack)
                {
                    SpectrumAnalyzer = ApplicationThemeManager.Instance.GetThemePack(((ThemePack) value).FileName);
                }
            }
        }

        #endregion

        public bool Equals(ApplicationDesign obj)
        {
            return ColorTheme.Equals(obj.ColorTheme) && BaseTheme.Equals(obj.BaseTheme) &&
                BackgroundImage != null && BackgroundImage.Equals(obj.BackgroundImage);
        }
    }
}