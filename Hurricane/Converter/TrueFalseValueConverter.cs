using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hurricane.Converter
{
    class TrueFalseValueConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            "Key", typeof (object), typeof (TrueFalseValueConverter), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register(
            "TrueValue", typeof (object), typeof (TrueFalseValueConverter), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register(
            "FalseValue", typeof (object), typeof (TrueFalseValueConverter), new PropertyMetadata(default(object)));

        public object FalseValue
        {
            get { return GetValue(FalseValueProperty); }
            set { SetValue(FalseValueProperty, value); }
        }

        public object TrueValue
        {
            get { return GetValue(TrueValueProperty); }
            set { SetValue(TrueValueProperty, value); }
        }

        public object Key
        {
            get { return GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == Key ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
