using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Hurricane.Music.Proxy;
using Hurricane.Settings;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;

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
            get { return Path.Combine(HurricaneSettings.Paths.BaseDirectory, "youtube-dl.exe"); }
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
        public async Task<Uri> GetYouTubeStreamUri(string youTubeLink)
        {
            await Load();
            using (var p = new Process
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
                    return await GetYouTubeStreamUri(youTubeLink);
                }
                if (!url.ToLower().StartsWith("error"))
                {
                    _tryagain = false;
                    return new Uri(url);
                }
                throw new Exception(url);
            }
        }

        public async Task<Stream> GetGroovesharkStream(string groovesharkUrl)
        {
            await Load();

            string streamUrl;
            GroovesharkApi.GroovesharkResult parameters;

            HttpProxy proxy = null;
            if (await GroovesharkApi.IsProxyRequired())
            {
                proxy = await ProxyManager.Instance.GetWebProxy();
            }
            
            using (var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "youtube-dl.exe",
                    Arguments = string.Format("-g {0}{1} -j", groovesharkUrl, proxy != null ? string.Format(" --proxy \"{0}\"", proxy) : null)
                }
            })
            {
                p.Start();
                streamUrl = await p.StandardOutput.ReadLineAsync();
                parameters = JsonConvert.DeserializeObject<GroovesharkApi.GroovesharkResult>(await p.StandardOutput.ReadToEndAsync());
            }

            if (string.IsNullOrEmpty(streamUrl))
            {
                Debug.Print("invalid proxy: " + proxy);
                ProxyManager.Instance.AddInvalid(proxy);
                return await GetGroovesharkStream(groovesharkUrl);
            }

            Debug.Print("everything is awesome");

            var data = Encoding.ASCII.GetBytes(parameters.HttpPostData);

            var request = (HttpWebRequest)WebRequest.Create(streamUrl);
            request.Headers.Add("Accept-Charset", parameters.HttpHeaders.AcceptCharset);
            request.Headers.Add("Accept-Language", parameters.HttpHeaders.AcceptLanguage);
            request.Headers.Add("Accept-Encoding", parameters.HttpHeaders.AcceptEncoding);
            request.Headers.Add("Cookie", parameters.HttpHeaders.Cookie);

            request.Method = "POST";
            request.ContentType = parameters.HttpHeaders.ContentType;
            request.ContentLength = data.Length;
            request.UserAgent = parameters.HttpHeaders.UserAgent;
            request.Accept = parameters.HttpHeaders.Accept;

            request.Proxy = proxy != null ? proxy.ToWebProxy() : null;

            var requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            var myHttpWebResponse = (HttpWebResponse)request.GetResponse();

            return myHttpWebResponse.GetResponseStream();
        }

        public async Task Download(string link, string fileName, Action<double> progressChangedAction)
        {
            await Load();
            using (var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = string.Format("{0} --extract-audio --ffmpeg-location \"{1}\" --output \"{2}\"", link, HurricaneSettings.Paths.FFmpegPath, fileName)
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

                if (!File.Exists(fileName)) throw new Exception();
            }
        }
    }
}