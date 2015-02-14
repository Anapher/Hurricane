using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Utilities
{
    static class GeneralHelper
    {
        /// <summary>
        /// Function to get file impression in form of string from a file location.
        /// </summary>
        /// <param name="filename">File Path to get file impression.</param>
        /// <returns>Byte Array</returns>
        public static string FileToMD5Hash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
        }

        public async static Task<bool> CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient { Proxy = null })
                using (var stream = await client.OpenReadTaskAsync("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRunningWithAdministratorRights()
        {
            var localAdminGroupSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            return windowsIdentity.Groups.Select(g => (SecurityIdentifier)g.Translate(typeof(SecurityIdentifier))).Any(s => s == localAdminGroupSid);
        }

        public static string EscapeFilename(string filenametoescape)
        {
            char[] illegalchars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            return RemoveChars(filenametoescape, illegalchars);
        }

        public static string EscapeArtistName(string artist)
        {
            char[] illegalchars = { '.' };
            return RemoveChars(artist, illegalchars);
        }

        public static string EscapeTitleName(string title)
        {
            char[] illegalchars = { '&' };
            return RemoveChars(title, illegalchars);
        }

        private static string RemoveChars(string content, char[] illegalchars)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            return illegalchars.Aggregate(content, (current, item) => current.Replace(item.ToString(), string.Empty));
        }

        public static void CreateShortcut(string path, string targetpath, string iconlocation)
        {
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(path);
                try
                {
                    lnk.TargetPath = targetpath;
                    lnk.IconLocation = iconlocation;
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        public static bool IsFileReady(String sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
