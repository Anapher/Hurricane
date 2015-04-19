using System.Diagnostics;
using Hurricane.Music.Track.WebApi;
using Hurricane.Music.Track.WebApi.SoundCloudApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.Services
{
    [TestClass]
    public class SoundCloudTests
    {
        [TestMethod]
        public void UrlDetection()
        {
            var api = new SoundCloudApi() as IMusicApi;
            var sw = Stopwatch.StartNew();

            var task1 = api.CheckForSpecialUrl("https://soundcloud.com/roccat-2/usher-yeah-techno-remix");
            task1.Wait();
            Trace.WriteLine(string.Format("detect valid music url and download information in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task1.Result.Item1);
            Assert.IsNotNull(task1.Result.Item2);
            Assert.IsNull(task1.Result.Item3);

            sw.Restart();
            var task2 = api.CheckForSpecialUrl("garcon");
            task2.Wait();
            Trace.WriteLine(string.Format("detect invalid url in {0} ticks", sw.ElapsedTicks));
            Assert.IsFalse(task2.Result.Item1);
            Assert.IsNull(task2.Result.Item2);
            Assert.IsNull(task2.Result.Item3);
        }

        [TestMethod]
        public void Search()
        {
            var api = new SoundCloudApi() as IMusicApi;
            var sw = Stopwatch.StartNew();

            var task1 = api.Search("eminem");
            task1.Wait();
            Trace.WriteLine(string.Format("valid soundcloud search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task1.Result != null && task1.Result.Count > 0);

            sw.Restart();
            var task2 = api.Search("H897g8983r34r&/R(%/&OBIUA=()/G=AD(||||-ojwrej");
            task2.Wait();
            Trace.WriteLine(string.Format("invalid soundcloud search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task2.Result.Count == 0);
        }
    }
}
