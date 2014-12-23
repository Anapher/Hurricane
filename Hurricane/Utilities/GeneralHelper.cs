using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hurricane.Utilities
{
    class GeneralHelper
    {
        /// <summary>
        /// Function to get file impression in form of string from a file location.
        /// </summary>
        /// <param name="_fileName">File Path to get file impression.</param>
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

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient() { Proxy = null})
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static string EscapeFilename(string filenametoescape)
        {
            char[] illegalchars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            return RemoveChars(filenametoescape, illegalchars);
        }

        public static string EscapeArtistName(string artist)
        {
            char[] illegalchars = new char[] { '.' };
            return RemoveChars(artist, illegalchars);
        }

        public static string EscapeTitleName(string title)
        {
            char[] illegalchars = new char[] { '&' };
            return RemoveChars(title, illegalchars);
        }

        protected static string RemoveChars(string content, char[] illegalchars)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            string result = content;
            foreach (var item in illegalchars)
            {
                result = result.Replace(item.ToString(), string.Empty);
            }

            return result;
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
