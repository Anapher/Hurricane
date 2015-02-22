using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Hurricane.Settings;
using Hurricane.Settings.Themes;

namespace Hurricane.Designer.Data
{
    public class BaseThemeData : DataThemeBase
    {
        public BaseThemeData()
        {
            ThemeSettings = new List<IThemeSetting>
            {
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"BlackColor\">(?<content>(.*?))<",
                    ID = "BlackColor",
                    DisplayName ="Black color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"WhiteColor\">(?<content>(.*?))<",
                    ID = "WhiteColor",
                    DisplayName ="White color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray1\">(?<content>(.*?))<",
                    ID = "Gray1",
                    DisplayName ="Gray #1"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray2\">(?<content>(.*?))<",
                    ID = "Gray2",
                    DisplayName ="Gray #2"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray7\">(?<content>(.*?))<",
                    ID = "Gray7",
                    DisplayName ="Gray #7"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray8\">(?<content>(.*?))<",
                    ID = "Gray8",
                    DisplayName ="Gray #8"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray10\" Color=\"(?<content>(.*?))\"",
                    ID = "Gray10",
                    DisplayName ="Gray #10"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"GrayNormal\">(?<content>(.*?))<",
                    ID = "GrayNormal",
                    DisplayName ="Gray normal"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"GrayHover\">(?<content>(.*?))<",
                    ID = "GrayHover",
                    DisplayName ="Gray hover"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"FlyoutColor\">(?<content>(.*?))<",
                    ID = "FlyoutColor",
                    DisplayName ="Flyout color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderValueDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderValueDisabled",
                    DisplayName ="Slider value disabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackDisabled",
                    DisplayName ="Slider track disabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderThumbDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderThumbDisabled",
                    DisplayName ="Slider thumb disabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderThumbDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderThumbDisabled",
                    DisplayName ="Slider thumb disabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackHover\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackHover",
                    DisplayName ="Slider track hover"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackNormal\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackNormal",
                    DisplayName ="Slider track normal"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"FlatButtonPressedBackgroundBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "FlatButtonPressedBackgroundBrush",
                    DisplayName ="Flat button pressed background brush"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuItemSelectionFill\" Color=\"(?<content>(.*?))\"",
                    ID = "MenuItemSelectionFill",
                    DisplayName ="Menu item selection fill"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuItemSelectionStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "MenuItemSelectionStroke",
                    DisplayName ="Menu item selection stroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemPressedFill\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemPressedFill",
                    DisplayName ="Top menu item pressed fill"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemPressedStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemPressedStroke",
                    DisplayName ="Top menu item pressed stroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemSelectionStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemSelectionStroke",
                    DisplayName ="Top menu item selection stroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"DisabledMenuItemForeground\" Color=\"(?<content>(.*?))\"",
                    ID = "DisabledMenuItemForeground",
                    DisplayName ="Diabled menu item foreground"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuShadowColor\">(?<content>(.*?))<",
                    ID = "MenuShadowColor",
                    DisplayName ="Menu shadow color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"PicturePreviewBackgroundBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "PicturePreviewBackgroundBrush",
                    DisplayName ="Preview picture background color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"PicturePreviewForegroundBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "PicturePreviewForegroundBrush",
                    DisplayName ="Preview picture foreground color"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderBackground\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderBackground",
                    DisplayName ="Slider background"
                },
                new ThemeColor
                {
                    RegexPattern ="x:Key=\"DisabledMenuItemGlyphPanel\" Color=\"(?<content>(.*?))\"",
                    ID ="DisabledMenuItemGlyphPanel",
                    DisplayName ="Disabled menu item glyph panel"
                }
            };
        }
        
        public static BaseThemeData LoadDefault()
        {
            var baseTheme = new BaseThemeData();
            baseTheme.LoadFromResourceDictionary(ApplicationThemeManager.Instance.BaseThemes.First(x => x.Name == "BaseLight").ResourceDictionary);
            return baseTheme;
        }

        public override string Source
        {
            get { return Properties.Resources.BaseTheme; }
        }

        public override string Filter
        {
            get { return string.Format("{0}|*.xaml", Application.Current.Resources["BaseTheme"]); }
        }

        public override string BaseDirectory
        {
            get { return HurricaneSettings.Instance.BaseThemesDirectory; }
        }
    }
}