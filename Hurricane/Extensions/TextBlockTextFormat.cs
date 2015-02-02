using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Hurricane.Extensions
{
    class TextBlockTextFormat
    {
        abstract class FormatRule
        {
            public abstract string RegexPattern { get; }
            public abstract IList<Run> GetRun(Match regexMatch);
        }

        class HeaderFormatRule : FormatRule
        {
            public override IList<Run> GetRun(Match regexMatch)
            {
                var result = new List<Run>();
                var run = new Run(regexMatch.Groups["text"].Value) {FontSize = 16, FontWeight = FontWeights.Bold};
                result.Add(run);
                return result;
            }

            public override string RegexPattern
            {
                get { return "^##(?<text>(.*?))$"; }
            }
        }

        class EnumerationRule : FormatRule
        {
            public override string RegexPattern
            {
                get { return "^- (?<text>(.*?))$"; }
            }

            public override IList<Run> GetRun(Match regexMatch)
            {
                return new List<Run> { new Run("• " + regexMatch.Groups["text"].Value) };
            }
        }

        class ItalicRule : FormatRule
        {
            public override string RegexPattern
            {
                get { return @"^\[i\](?<text>(.*?))$"; }
            }

            public override IList<Run> GetRun(Match regexMatch)
            {
                return new List<Run> { new Run(regexMatch.Groups["text"].Value) { FontStyle = FontStyles.Italic } };
            }
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof (string), typeof (TextBlockTextFormat), new PropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBlock = dependencyObject as TextBlock;
            if (textBlock == null) throw new ArgumentException();
            var rules = new List<FormatRule> { new HeaderFormatRule(), new EnumerationRule(), new ItalicRule() };

            var inlines = textBlock.Inlines;
            inlines.Clear();

            foreach (var line in dependencyPropertyChangedEventArgs.NewValue.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                bool success = false;
                foreach (var collection in from rule in rules let regex = new Regex(rule.RegexPattern) let match = regex.Match(line) where match.Success select rule.GetRun(match))
                {
                    inlines.AddRange(collection);
                    success = true;
                    break;
                }
                if (!success) inlines.Add(new Run(line));
                inlines.Add(Environment.NewLine);
            }
        }

        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string) element.GetValue(FormattedTextProperty);
        }
    }
}
