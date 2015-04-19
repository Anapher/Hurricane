using System.Diagnostics;
using Hurricane.Music.Track;
using Hurricane.Music.Track.WebApi.YouTubeApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.Services
{
    [TestClass]
    public class YouTubeTests
    {
        private const string VideoPlaylistTestUrl =
            "https://www.youtube.com/watch?v=8cxdHeq-7zs&index=3&list=PL14B46F762CD81845&ab_channel=KellyMissesVlog";
        private const string VideoTestUrl1 = "https://www.youtube.com/v/8cxdHeq-7zs";
        private const string VideoTestUrl2 = "https://www.youtube.com/watch?v=8cxdHeq-7zs";
        private const string PlaylistUrl = "https://www.youtube.com/playlist?list=PL14B46F762CD81845";

        [TestMethod]
        public void YouTubeIdFiltering()
        {
            const string id = "8cxdHeq-7zs";

            Assert.AreEqual(id, YouTubeTrack.GetYouTubeIdFromLink(VideoPlaylistTestUrl));
            Assert.AreEqual(id, YouTubeTrack.GetYouTubeIdFromLink(VideoTestUrl1));
            Assert.AreEqual(id, YouTubeTrack.GetYouTubeIdFromLink(VideoTestUrl2));
        }

        [TestMethod]
        public void UrlDetection()
        {
            var api = new YouTubeApi();
            var sw = Stopwatch.StartNew();

            var task1 = api.CheckForSpecialUrl(VideoTestUrl1);
            task1.Wait();
            Trace.WriteLine(string.Format("detect valid video url and download information in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task1.Result.Item1);
            Assert.IsNotNull(task1.Result.Item2);
            Assert.IsNull(task1.Result.Item3);

            sw.Restart();
            var task2 = api.CheckForSpecialUrl(VideoTestUrl2);
            task2.Wait();
            Trace.WriteLine(string.Format("detect valid video url and download information in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task2.Result.Item1);
            Assert.IsNotNull(task2.Result.Item2);
            Assert.IsNull(task2.Result.Item3);

            sw.Restart();
            var task3 = api.CheckForSpecialUrl(VideoPlaylistTestUrl);
            task3.Wait();
            Trace.WriteLine(string.Format("detect valid playlist url and download information in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task3.Result.Item1);
            Assert.IsNotNull(task3.Result.Item2);
            Assert.IsNotNull(task3.Result.Item3);

            sw.Restart();
            var task4 = api.CheckForSpecialUrl(PlaylistUrl);
            task4.Wait();
            Trace.WriteLine(string.Format("detect valid playlist url and download information in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task4.Result.Item1);
            Assert.IsNotNull(task4.Result.Item2);
            Assert.IsNotNull(task4.Result.Item3);

            sw.Restart();
            var task5 = api.CheckForSpecialUrl("garcon");
            Trace.WriteLine(string.Format("detect invalid url in {0} ticks", sw.ElapsedTicks));
            Assert.IsFalse(task5.Result.Item1);
        }

        [TestMethod]
        public void Search()
        {
            var api = new YouTubeApi();
            var sw = Stopwatch.StartNew();

            var task1 = api.Search("eminem");
            task1.Wait();
            Trace.WriteLine(string.Format("valid youtube search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task1.Result != null && task1.Result.Count > 0);

            sw.Restart();
            var task2 = api.Search("H897g8983r34r&/R(%/&OBIUA=()/G=AD(||||-ojwrej");
            task2.Wait();
            Trace.WriteLine(string.Format("invalid youtube search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task2.Result.Count == 0);
        }
    }
}
