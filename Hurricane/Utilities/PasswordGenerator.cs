using System;
using System.Security.Cryptography;
using System.Text;

namespace Hurricane.Utilities
{
    public class PasswordGenerator
    {
        private static string _handshake;
        public static string GetSystemSpecificHandshake()
        {
            return _handshake ??
                   (_handshake = GetMD5Hash(Environment.UserName + Environment.MachineName).Substring(0, 16));
                //16 chars are long enough
        }

        // ReSharper disable once InconsistentNaming
        private static string GetMD5Hash(string textToHash)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(textToHash))).Replace("-", "");
            }
        }
    }
}
