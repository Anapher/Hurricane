using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hurricane.Settings;
using Hurricane.Utilities;
using Hurricane.Utilities.Web;
using Newtonsoft.Json;

namespace Hurricane.Music.Track.WebApi.VkontakteApi
{
    public class VkontakteApi : IMusicApi, INotifyPropertyChanged
    {
        public PasswordEntry Credentials { get; set; }
        private string _accessToken;
        private DateTime _nextTokenRefresh;

        public async Task<Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>> CheckForSpecialUrl(string url)
        {
            return new Tuple<bool, List<WebTrackResultBase>, IPlaylistResult>(false, null, null);
        }

        public string ServiceName
        {
            get { return "Vkontakte"; }
        }

        public Task<List<WebTrackResultBase>> Search(string searchText)
        {
            return Search(searchText, Credentials.Field1, Credentials.Field2);
        }

        public override string ToString()
        {
            return ServiceName;
        }

        public bool IsEnabled
        {
            get { return !string.IsNullOrEmpty(Credentials.Field2); }
        }

        public async Task<List<WebTrackResultBase>> Search(string searchText, string email, string password)
        {
            await CheckAccessToken(email, password);

            using (var wc = new WebClient { Proxy = null })
            {
                var str = await
                    wc.DownloadStringTaskAsync(
                        new Uri(
                            string.Format(
                                "https://api.vk.com/method/audio.search?q={0}&auto_complete={1}&sort={2}&lyrics={3}&count={4}&offset={5}&access_token={6}",
                                searchText.ToEscapedUrl(),
                                "true", 2, "false", 50, 0, _accessToken)));
                var count = uint.Parse(Regex.Match(str, @"\[(?<count>(\d+))(?:,|\])").Groups["count"].Value);
                if (count == 0) return new List<WebTrackResultBase>();

                var cutText = str.Replace(count + ",", null);
                var result = JsonConvert.DeserializeObject<SearchResult>(cutText);

                if (result == null || result.response.Count == 0) return new List<WebTrackResultBase>();

                return result.response.Select(x => new VkontakteWebTrackResult
                {
                    Duration = TimeSpan.FromSeconds(x.duration),
                    Uploader = x.artist,
                    Url = x.url,
                    Title = x.title.Replace("\n", " "),
                    SearchResult = x
                }).Cast<WebTrackResultBase>().ToList();
            }
        }

        private async Task CheckAccessToken(string email, string password)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _nextTokenRefresh)
                return;
            var authorization = await GetAccessToken(email, password);
            _nextTokenRefresh = DateTime.Now.AddSeconds(double.Parse(authorization.ExpiresIn));
            _accessToken = authorization.AccessToken;
        }

        private async Task<VkAuthorization> GetAccessToken(string email, string password, long? captchaSid = null, string captchaKey = null)
        {
            var url = string.Format("https://oauth.vk.com/authorize?client_id={0}&scope=audio&response_type=token", SensitiveInformation.VkAppId);
            var authorizeUrlResult = WebCall.MakeCall(url);
            var loginForm = await Task.Run(() => WebForm.From(authorizeUrlResult).WithField("email").FilledWith(email).And().WithField("pass").FilledWith(password));
            if (captchaSid.HasValue)
                loginForm.WithField("captcha_sid").FilledWith(captchaSid.Value.ToString()).FilledWith("captcha_key").FilledWith(captchaKey);
            var loginFormPostResult = await Task.Run(() => WebCall.Post(loginForm));

            var authorization = VkAuthorization.From(loginFormPostResult.ResponseUrl);
            if (authorization.CaptchaId.HasValue)
                throw new CaptchaNeededException(authorization.CaptchaId.Value, "http://api.vk.com/captcha.php?sid=" + authorization.CaptchaId.Value);

            if (!authorization.IsAuthorizationRequired)
                return authorization;

            var authorizationForm = await Task.Run(() => WebForm.From(loginFormPostResult));
            var authorizationFormPostResult = await Task.Run(() => WebCall.Post(authorizationForm));

            return VkAuthorization.From(authorizationFormPostResult.ResponseUrl);
        }

        public System.Windows.FrameworkElement ApiSettings
        {
            get { return new Settings(this); }
        }

        public const string Id = "vk";
        public VkontakteApi()
        {
            Credentials = HurricaneSettings.Instance.Config.Passwords.FirstOrDefault(x => x.Id == Id) ??
                          new PasswordEntry {Id = Id};
        }

        public VkontakteApi(string email, string password)
        {
            Credentials = new PasswordEntry {Field1 = email, Field2 = password, Id = Id};
        }

        public void OnIsEnabledChanged()
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}