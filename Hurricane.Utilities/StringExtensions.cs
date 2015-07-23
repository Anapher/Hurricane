using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Hurricane.Utilities
{
    public static class StringExtensions
    {
        /*
        public static string ToFileDialogFilter(this IList<string> list, string allFilesText)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{allFilesText} (*.*)|*.*");
            foreach (var extension in list)
            {
                stringBuilder.Append($"|{extension.ToUpper()} (*.)");
            }
        }*/

        /// <summary>
        /// Replace empty childs with nothing
        /// </summary>
        /// <param name="s">The invalid json</param>
        /// <returns>Returns a good json string</returns>
         public static string FixJsonString(this string s)
        {
            return Regex.Replace(s, @"""[a-zA-Z]+?""\s*?:\s*?""(\\n|\\t|\s)+?""\s*?,?", string.Empty,
                RegexOptions.Singleline);
        }

        public static string ToMd5Hash(this string s)
        {
            using (var md5 = MD5.Create())
                return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", null).ToUpper();
        }
    }
}