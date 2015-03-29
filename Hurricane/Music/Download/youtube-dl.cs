using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Settings;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.Music.Download
{
    class youtube_dl
    {
        #region "Singleton & Constructor"

        private static youtube_dl _instance;
        public static youtube_dl Instance
        {
            get { return _instance ?? (_instance = new youtube_dl()); }
        }


        private youtube_dl()
        {
        }

        #endregion

        public string ExecutablePath
        {
            get { return Path.Combine(HurricaneSettings.Instance.BaseDirectory, "youtube-dl.exe"); }
        }

        private bool _isLoaded;

        public async Task Load()
        {
            if (_isLoaded || !HurricaneSettings.Instance.Config.CheckForYoutubeDlUpdates) return;

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = ExecutablePath,
                    Arguments = "-U",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };

            p.Start(); //Updating to version 2015.01.16 ...
            var info = await p.StandardOutput.ReadLineAsync();
            var regex = new Regex(@"Updating to version (?<version>(.*?)) \.\.\.");
            var match = regex.Match(info);
            if (match.Success)
            {
                var metrowindow = (MetroWindow)Application.Current.MainWindow;
                var controller = await metrowindow.ShowProgressAsync(Application.Current.Resources["UpdateYoutubedl"].ToString(), string.Format(Application.Current.Resources["UpdateYoutubedlMessage"].ToString(), match.Groups["version"].Value));
                controller.SetIndeterminate();
                await Task.Run(() => p.WaitForExit());
                await controller.CloseAsync();
            }
            _isLoaded = true;
        }

        private bool _tryagain;
        public async Task<Uri> GetStreamUri(string youTubeLink)
        {
            await Load();
            using (var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = string.Format("-g {0} -f bestaudio", youTubeLink)
                }
            })
            {
                p.Start();
                var url = await p.StandardOutput.ReadToEndAsync();
                if (string.IsNullOrEmpty(url))
                {
                    if (_tryagain)
                    {
                        _tryagain = false; throw new Exception(url);
                    }
                    _tryagain = true;
                    return await GetStreamUri(youTubeLink);
                }
                if (!url.ToLower().StartsWith("error"))
                {
                    _tryagain = false;
                    return new Uri(url);
                }
                throw new Exception(url);
            }
        }

        public async Task<string> DownloadYouTubeVideo(string youTubeLink, string fileNameWithoutExtension, Action<double> progressChangedAction)
        {
            await Load();
            var file = fileNameWithoutExtension + ".m4a";
            using (var p = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = string.Format("{0} --extract-audio --output \"{1}\"", youTubeLink, file)
                }
            })
            {
                p.Start();

                var regex = new Regex(@"^\[download\].*?(?<percentage>(.*?))% of"); //[download]   2.7% of 4.62MiB at 200.00KiB/s ETA 00:23
                while (!p.HasExited)
                {
                    var line = await p.StandardOutput.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        var doub = double.Parse(match.Groups["percentage"].Value, CultureInfo.InvariantCulture);
                        progressChangedAction.Invoke(doub);
                    }
                }

                if (!File.Exists(file)) throw new Exception();
                return file;
            }
        }
    }
}