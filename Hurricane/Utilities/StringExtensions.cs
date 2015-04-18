using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Hurricane.Utilities
{
    public static class StringExtensions
    {
        public static string ToLowercaseNamingConvention(this string s)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(s, "_").ToLower();
        }

        /// <summary>
        /// Remove all invalid chars for a file name
        /// </summary>
        /// <param name="fileNameToEscape">The name of the file which could contain invalid chars</param>
        /// <returns>The <see cref="fileNameToEscape"/> without the invalid chars</returns>
        public static string ToEscapedFilename(this string fileNameToEscape)
        {
            char[] illegalchars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            return RemoveChars(fileNameToEscape, illegalchars);
        }

        /// <summary>
        /// Remove all invalid chars for a url
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToEscapedUrl(this string input)
        {
            return HttpUtility.UrlEncode(input);
        }

        private static string RemoveChars(string content, IEnumerable<char> illegalchars)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            return illegalchars.Aggregate(content, (current, item) => current.Replace(item.ToString(), string.Empty));
        }
    }
}