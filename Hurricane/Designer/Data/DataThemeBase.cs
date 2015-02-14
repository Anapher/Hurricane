using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;

namespace Hurricane.Designer.Data
{
    public abstract class DataThemeBase : ISaveable
    {
        public List<IThemeSetting> ThemeSettings { get; set; }
        public string Name { get; set; }
        public abstract string Source { get; }

        public void LoadFromFile(string filePath)
        {
            LoadFromString(File.ReadAllText(filePath));
        }

        public void LoadFromString(string content)
        {
            foreach (var setting in ThemeSettings)
            {
                setting.SetValue(Regex.Match(content, setting.RegexPattern).Groups["content"].Value);
            }
        }

        public ResourceDictionary GetResourceDictionary()
        {
            return (ResourceDictionary)XamlReader.Parse(ToString());
        }

        public override string ToString()
        {
            return ThemeSettings.Aggregate(Source, (current, color) => current.Replace("{" + color.ID + "}", color.Value));
        }

        public void Save(string path)
        {
            File.WriteAllText(path, ToString());
        }
    }
}