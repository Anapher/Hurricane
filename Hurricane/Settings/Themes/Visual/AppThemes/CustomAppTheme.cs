using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace Hurricane.Settings.Themes.Visual.AppThemes
{
    [Serializable]
    public class CustomAppTheme : AppThemeBase
    {
        public static bool FromFile(string filename, out CustomAppTheme result)
        {
            var appTheme = new CustomAppTheme { Name = Path.GetFileNameWithoutExtension(filename) };

            try
            {
                if (!IsAppThemeDictionary(appTheme.ResourceDictionary))
                {
                    result = null;
                    return false;
                }
            }
            catch (XamlParseException)
            {
                result = null;
                return false;
            }

            result = appTheme;
            return true;
        }

        [XmlIgnore]
        public override ResourceDictionary ResourceDictionary
        {
            get { return new ResourceDictionary { Source = new Uri(Path.Combine(HurricaneSettings.Instance.AppThemesDirectory, Name + ".xaml"), UriKind.RelativeOrAbsolute) }; }
        }

        public override string TranslatedName
        {
            get { return Name; }
        }

        public override string Group
        {
            get { return Application.Current.Resources["Custom"].ToString(); }
        }

        public static bool IsAppThemeDictionary(ResourceDictionary resources)
        {
            if (resources == null) throw new ArgumentNullException("resources");

            // Note: add more checks if these keys aren't sufficient
            var styleKeys = new List<string>(new[]
            {
                "BlackColor",
                "WhiteColor",
                "Gray1",
                "Gray2",
                "Gray7",
                "Gray8",
                "Gray10",
                "GrayNormal",
                "GrayHover",
                "FlyoutColor",
            });

            foreach (var styleKey in styleKeys)
            {
                // Note: do not use contains, because that will look in all merged dictionaries as well. We need to check
                // out the actual keys of the current resource dictionary
                if (!(from object resourceKey in resources.Keys
                      select resourceKey as string).Any(keyAsString => string.Equals(keyAsString, styleKey)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}