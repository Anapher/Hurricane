using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace Hurricane.Extensions.Converter
{
    class TextWithHyperlinkParametersConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TextBlock textblock = new TextBlock();
            string stringtoformat = values[0].ToString();

            int counter = 0;
            foreach (var item in Regex.Split(stringtoformat, @"\{.\}"))
            {
                counter++;
                textblock.Inlines.Add(new Run(item));

                if (values.Length - 1 < counter) continue;
                string[] sSplit = values[counter].ToString().Split('$');
                string text = sSplit[0];
                string url = sSplit[1];

                Hyperlink hyperlink = new Hyperlink(new Run(text)) { NavigateUri = new Uri(url) };
                hyperlink.RequestNavigate += (s, e) => { System.Diagnostics.Process.Start(e.Uri.AbsoluteUri); };
                textblock.Inlines.Add(hyperlink);
            }
            return textblock;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
