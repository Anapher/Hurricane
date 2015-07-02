using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Utilities
{
    public static class GeneralHelper
    {
        /// <summary>
        /// Function to get file impression in form of string from a file location.
        /// </summary>
        /// <param name="filename">File Path to get file impression.</param>
        /// <returns>Byte Array</returns>
        // ReSharper disable once InconsistentNaming
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

        /// <summary>
        /// Check if the internet is available. It connects to google.com
        /// </summary>
        /// <returns>If the connection to google was successful</returns>
        public async static Task<bool> CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient { Proxy = null })
                using (await client.OpenReadTaskAsync("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the current user has administrator rights
        /// </summary>
        /// <returns>If the current user has administrator rights</returns>
        public static bool IsRunningWithAdministratorRights()
        {
            var localAdminGroupSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity == null || windowsIdentity.Groups == null) return false;
            return windowsIdentity.Groups.Select(g => (SecurityIdentifier)g.Translate(typeof(SecurityIdentifier))).Any(s => s == localAdminGroupSid);
        }

        /// <summary>
        /// Creates a new short cut for to the <see cref="targetPath"/>
        /// </summary>
        /// <param name="path">The save location for the shortcut</param>
        /// <param name="targetPath">The destination of the shortcut</param>
        /// <param name="iconLocation">The location of the icon for the shortcut</param>
        public static void CreateShortcut(string path, string targetPath, string iconLocation)
        {
            var t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(path);
                try
                {
                    lnk.TargetPath = targetPath;
                    lnk.IconLocation = iconLocation;
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

        /// <summary>
        /// Check if the file can be opened
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        /// <returns>If the file can be opened</returns>
        public static bool IsFileReady(string fileName)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the file is a video (for Hurricane)
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        /// <returns>If the file is a video</returns>
        public static bool IsVideo(string fileName)
        {
            return fileName.EndsWith(".mp4") || fileName.EndsWith(".wmv");
        }

        /// <summary>
        /// Builds the secounds part of a filter entry by the array and adds the missing dots: .mp4|mp3|.wmv|m4a -> .mp4;.mp3;.wmv;.m4a
        /// </summary>
        /// <param name="extensions">The list of the extensions</param>
        /// <returns>The string which contains the extensions, ready for the dialog filter</returns>
        public static string GetFileDialogFilterFromArray(IEnumerable<string> extensions)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Concat(extensions.Select(x => (x.StartsWith("*.") ? null : (x.StartsWith(".") ? "*" : "*.")) + x + ";").ToArray()));
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generates a free file name in the give <see cref="directory"/> and with the <see cref="extension"/>
        /// </summary>
        /// <param name="directory">The directory where the file should be created</param>
        /// <param name="extension">The extension of the file</param>
        /// <returns>A free random file name with the <see cref="extension"/></returns>
        public static FileInfo GetFreeFileName(DirectoryInfo directory, string extension)
        {
            while (true)
            {
                var file = new FileInfo(Path.Combine(directory.FullName, Guid.NewGuid() + (extension.StartsWith(".") ? null : ".") + extension));
                if (file.Exists) continue;
                return file;
            }
        }

        /// <summary>
        /// Check if the path is a file or a folder
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>True if the path is a file</returns>
        public static bool PathIsFile(string path)
        {
            var attr = File.GetAttributes(path);
            return (attr & FileAttributes.Directory) != FileAttributes.Directory;
        }

        public static string GetFilePathWithoutExtension(string path)
        {
            return Path.Combine(path.Substring(0, path.LastIndexOf("\\", StringComparison.Ordinal)),
                Path.GetFileNameWithoutExtension(path));
        }
        
        /// <summary>
        /// Decodes the input with the ROT13 algorithm
        /// </summary>
        /// <param name="input">The ROT13 encrypted string</param>
        /// <returns>The decrypted string</returns>
        // ReSharper disable once InconsistentNaming
        public static string ROT13(string input)
        {
            return !string.IsNullOrEmpty(input) ? new string(input.ToCharArray().Select(s => (char)((s >= 97 && s <= 122) ? ((s + 13 > 122) ? s - 13 : s + 13) : (s >= 65 && s <= 90 ? (s + 13 > 90 ? s - 13 : s + 13) : s))).ToArray()) : input;
        }
    }
}