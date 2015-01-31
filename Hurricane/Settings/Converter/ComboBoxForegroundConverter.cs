using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Hurricane.Music.Data;
using Hurricane.Utilities;

namespace Hurricane.Settings.Converter
{
    class ComboBoxForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as FrameworkElement;
            if (item == null) return false;
            var comboboxitem = UIHelper.FindUpVisualTree<ComboBoxItem>(item);
            return comboboxitem != null && comboboxitem.IsSelected;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}