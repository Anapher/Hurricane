using System.Collections.Generic;

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
                    ID = "BlackColor"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"WhiteColor\">(?<content>(.*?))<",
                    ID = "WhiteColor"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray1\">(?<content>(.*?))<",
                    ID = "Gray1"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray2\">(?<content>(.*?))<",
                    ID = "Gray2"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray7\">(?<content>(.*?))<",
                    ID = "Gray7"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray8\">(?<content>(.*?))<",
                    ID = "Gray8"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"Gray10\" Color=\"(?<content>(.*?))\"",
                    ID = "Gray10"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"GrayNormal\">(?<content>(.*?))<",
                    ID = "GrayNormal"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"GrayHover\">(?<content>(.*?))<",
                    ID = "GrayHover"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"FlyoutColor\">(?<content>(.*?))<",
                    ID = "FlyoutColor"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderValueDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderValueDisabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackDisabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderThumbDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderThumbDisabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderThumbDisabled\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderThumbDisabled"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackHover\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackHover"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"SliderTrackNormal\" Color=\"(?<content>(.*?))\"",
                    ID = "SliderTrackNormal"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"FlatButtonPressedBackgroundBrush\" Color=\"(?<content>(.*?))\"",
                    ID = "FlatButtonPressedBackgroundBrush"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuItemSelectionFill\" Color=\"(?<content>(.*?))\"",
                    ID = "MenuItemSelectionFill"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuItemSelectionStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "MenuItemSelectionStroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemPressedFill\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemPressedFill"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemPressedStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemPressedStroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"TopMenuItemSelectionStroke\" Color=\"(?<content>(.*?))\"",
                    ID = "TopMenuItemSelectionStroke"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"DisabledMenuItemForeground\" Color=\"(?<content>(.*?))\"",
                    ID = "DisabledMenuItemForeground"
                },
                new ThemeColor
                {
                    RegexPattern = "x:Key=\"MenuShadowColor\">(?<content>(.*?))<",
                    ID = "MenuShadowColor"
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
                new ThemeBoolean
                {
                    RegexPattern ="x:Key=\"LightPlaceHolder\">(?<content>(.*?))<",
                    ID ="LightPlaceHolder",
                    DisplayName ="Use light place holder"
                },
                new ThemeBoolean
                {
                    RegexPattern ="x:Key=\"LightVolumeIcon\">(?<content>(.*?))<",
                    ID ="LightVolumeIcon",
                    DisplayName ="Use light volume icon"
                },
                new ThemeBoolean
                {
                    RegexPattern ="x:Key=\"LightVolumeIcon\">(?<content>(.*?))<",
                    ID ="UseDialogsForWhiteTheme",
                    DisplayName ="Use dialogs for white themes"
                }
            };
        }

        public override string Source
        {
            get { return Properties.Resources.BaseTheme; }
        }
    }
}