using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Documents;

namespace Hurricane.Converter
{
    class HtmlToInlinesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var htmlText = value as string;
            if (string.IsNullOrWhiteSpace(htmlText))
                return value;

            var splits = Regex.Split(htmlText, "<a ");
            var result = new List<Inline>();
            foreach (var split in splits)
            {
                if (!split.StartsWith("href=\""))
                {
                    result.AddRange(FormatText(split));
                    continue;
                }

                var match = Regex.Match(split, "^href=\"(?<url>(.+?))\" class=\".+?\">(?<text>(.*?))</a>(?<content>(.*?))$", RegexOptions.Singleline);
                if (!match.Success)
                {
                    result.Add(new Run(split));
                    continue;
                }
                var hyperlink = new Hyperlink(new Run(match.Groups["text"].Value))
                {
                    NavigateUri = new Uri(match.Groups["url"].Value)
                };
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                result.Add(hyperlink);

                result.AddRange(FormatText(match.Groups["content"].Value));
            }
            return result;
        }

        private static IEnumerable<Inline> FormatText(string text)
        {
            var split = text.Split(new []{ "&quot;" }, StringSplitOptions.None);
            bool foo = false;
            foreach (var s in split)
            {
                if (foo)
                {
                    yield return new Italic(new Run(s));
                    foo = false;
                }
                else
                {
                    yield return new Run(s);
                    foo = true;
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
