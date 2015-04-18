using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Hurricane.Utilities
{
    public static class StringExtensions
    {
        public static string ToSentenceCase(this string s)
        {
            return Regex.Replace(s, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToUpper(m.Value[1]));
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