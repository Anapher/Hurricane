using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace Hurricane.Designer.Data
{
    public abstract class DataThemeBase : ISaveable
    {
        public List<IThemeSetting> ThemeSettings { get; set; }
        public string Name { get; set; }
        public abstract string Source { get; }
        public abstract string Filter { get; }
        public abstract string BaseDirectory { get; }

        public void LoadFromFile(string filePath)
        {
            LoadFromResourceDictionary(new ResourceDictionary {Source = new Uri(filePath)});
        }

        public void LoadFromResourceDictionary(ResourceDictionary dictionary)
        {
            foreach (var setting in ThemeSettings)
            {
                setting.SetValue(GetValueFromDictionary(setting.ID, dictionary).ToString());
            }
        }

        private object GetValueFromDictionary(string key, ResourceDictionary dictionary)
        {
            if (dictionary.Contains(key))
            {
                return dictionary[key];
            }
            foreach (var resourceDictionary in dictionary.MergedDictionaries)
            {
                var result = GetValueFromDictionary(key, resourceDictionary);
                if (result != null) return result;
            }
            return null;
        }

        public ResourceDictionary GetResourceDictionary()
        {
            var toString = ToString();
            return (ResourceDictionary)XamlReader.Parse(toString);
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