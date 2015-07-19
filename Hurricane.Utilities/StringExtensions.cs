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
            return Regex.Replace(s, @"""[a-zA-Z]+?""\s*?:\s*?""[\\n|\\r|\s]+?""\s*?,", string.Empty, RegexOptions.Singleline);
        }
    }
}