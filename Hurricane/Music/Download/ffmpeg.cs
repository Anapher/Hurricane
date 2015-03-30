using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Hurricane.Settings;

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
        public static Task ConvertFile(string fileName, string newFileName, DownloadSettings settings)
        {
            return ConvertFile(fileName, newFileName, settings.Bitrate, settings.Format);
        }

        /// <summary>
        /// Converts the file
        /// </summary>
        /// <param name="fileName">The path to the file which should become converted</param>
        /// <param name="newFileName">The name of the new file WITHOUT extension</param>
        /// <param name="bitrate">The audio bitrate</param>
        /// <param name="format"></param>
        public static async Task ConvertFile(string fileName, string newFileName, AudioBitrate bitrate, AudioFormat format)
        {
            var fileToConvert = new FileInfo(fileName);

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = HurricaneSettings.Paths.FFmpegPath,
                    Arguments = GetParameter(fileName, newFileName, bitrate, format),
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            p.Start();
            await Task.Run(() => p.WaitForExit());
            var newFile = new FileInfo(newFileName);

            if (!newFile.Exists || newFile.Length == 0)
            {
                if (newFile.Exists) newFile.Delete();
                fileToConvert.MoveTo(newFileName); //If the convert failed, we just use the "old" file
            }

            fileToConvert.Delete();
        }

        private static string GetParameter(string inputFile, string outputFile, AudioBitrate bitrate, AudioFormat format)
        {
            return string.Format("-i \"{0}\" -c:a {1} -vn -q:a 0 -b:a {2}k \"{3}\"", inputFile, GetAudioLibraryFromFormat(format), bitrate.ToString().Remove(0, 1), outputFile);
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum AudioFormat
    {
        Copy,
        MP3,
        AAC,
        WMA
    }

    public enum AudioBitrate
    {
        [Description("64 kbit/s")]
        B64,
        [Description("96 kbit/s")]
        B96,
        [Description("128 kbit/s")]
        B128,
        [Description("160 kbit/s")]
        B160,
        [Description("192 kbit/s")]
        B192,
        [Description("256 kbit/s")]
        B256,
        [Description("320 kbit/s")]
        B320
    }
}
