using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Hurricane.Settings.Converter
{
    public class GroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var lst = (IList) value;
            var list = lst.Cast<IGroupable>().ToList();

            var lcv = new ListCollectionView(list.OrderBy(w => w.Group).ToList());
            var firstItem = list.First();
            if (list.All(x => x.Group == firstItem.Group)) return list; //If all are in the same group, we don't need groups

            if (lcv.GroupDescriptions != null) lcv.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public interface IGroupable
    {
        string Group { get; } 
    }
}