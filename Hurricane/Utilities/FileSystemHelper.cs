using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Hurricane.Utilities
{
    public static class FileSystemHelper
    {
        /// <summary>
        /// Returns the absolute path from a relative/absolute path
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="baseFolder">If the path is relativ, the path to the containing directory</param>
        /// <returns>An awesome, absolute path</returns>
        public static string GetAbsolutePath(string path, string baseFolder)
        {
            if (Regex.IsMatch(path.ToUpper(), "^[A-Z]:\\\\"))
                return path;
            return Path.Combine(baseFolder, path.StartsWith(@"\") ? path.Substring(1) : path);
        }

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
    }
}