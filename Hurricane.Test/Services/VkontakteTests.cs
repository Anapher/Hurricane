using System.Diagnostics;
using System.IO;
using Hurricane.Music.Track.WebApi.VkontakteApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.Services
{
    [TestClass]
    public class VkontakteTests
    {
        [TestMethod]
        public void Search()
        {
            var loginFile = new FileInfo(@"..\..\LoginData\vkontakte.txt");
            if(!loginFile.Exists)
                Assert.Fail("Login information not found. Please create the file \"vkontakte.txt\" in LoginData with your information to run this test.");
            var split = File.ReadAllText(loginFile.FullName).Trim().Split(':');

            var api = new VkontakteApi(split[0], split[1]);
            var sw = Stopwatch.StartNew();

            var task1 = api.Search("eminem");
            task1.Wait();
            Trace.WriteLine(string.Format("valid Vkontakte search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task1.Result != null && task1.Result.Count > 0);

            sw.Restart();
            var task2 = api.Search("H897g8983r34r&/R(%/&OBIUA=()/G=AD(||||-ojwrej");
            task2.Wait();
            Trace.WriteLine(string.Format("invalid Vkontakte search in {0} ms", sw.ElapsedMilliseconds));
            Assert.IsTrue(task2.Result.Count == 0);
        }
    }
}
