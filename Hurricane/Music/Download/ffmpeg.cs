using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Hurricane.Music.Download
{
    class ffmpeg
    {
        /// <summary>
        /// Converts the file
        /// </summary>
        /// <param name="fileName">The path to the file which should become converted</param>
        /// <param name="newFileName">The name of the new file WITHOUT extension</param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static Task<string> ConvertFile(string fileName, string newFileName, ConverterSettings settings)
        {
            return ConvertFile(fileName, newFileName, (settings.Quality - 10) * -1, settings.Format);
        }

        /// <summary>
        /// Converts the file
        /// </summary>
        /// <param name="fileName">The path to the file which should become converted</param>
        /// <param name="newFileName">The name of the new file WITHOUT extension</param>
        /// <param name="quality">The audio quality (0 - 10), 0 ist the best</param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static async Task<string> ConvertFile(string fileName, string newFileName, int quality, AudioFormat format)
        {
            var fileToConvert = new FileInfo(fileName);

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = "ffmpeg.exe",
                    Arguments = GetParameter(fileName, newFileName, quality, format),
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            p.Start();
            await Task.Run(() => p.WaitForExit());
            var newFile = new FileInfo(newFileName + GetAudioExtension(fileName, format));

            if (!newFile.Exists || newFile.Length == 0)
            {
                if (newFile.Exists) newFile.Delete();
                fileToConvert.MoveTo(Path.Combine(fileToConvert.Directory.FullName, Path.GetFileNameWithoutExtension(newFileName) + fileToConvert.Extension)); //If the convert failed, we just use the "old" file
                return fileToConvert.FullName;
            }

            fileToConvert.Delete();
            return newFile.FullName;
        }

        private static string GetParameter(string inputFile, string outputFile, int quality, AudioFormat format)
        {
            return string.Format("-i \"{0}\" -c:a {1} -vn -q:a {2} \"{3}{4}\"", inputFile, GetAudioLibraryFromFormat(format), quality, outputFile, GetAudioExtension(inputFile, format ));
        }

        public static string GetAudioLibraryFromFormat(AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.Copy:
                    return "copy";
                case AudioFormat.MP3:
                    return "libmp3lame"; //works
                case AudioFormat.AAC:
                    return "libvo_aacenc";
                case AudioFormat.WMA:
                    return "wmav2";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetAudioExtension(string inputFile, AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.Copy:
                    return new FileInfo(inputFile).Extension;
                case AudioFormat.MP3:
                    return ".mp3";
                case AudioFormat.AAC:
                    return ".aac";
                case AudioFormat.WMA:
                    return ".wma";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
