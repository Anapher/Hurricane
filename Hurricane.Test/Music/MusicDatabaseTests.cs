using Hurricane.Music.MusicCover.APIs.Lastfm;
using Hurricane.Music.Track;
using Hurricane.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.Music
{
    [TestClass]
    public class MusicDatabaseTests
    {
        [TestMethod]
        public void CheckLastfm()
        {
            var task = LastfmApi.GetImage(ImageQuality.Maximum, false, null,
                new LocalTrack{Title ="Eminem - Beautiful"}, false);
            task.Wait();
            Assert.IsNotNull(task.Result);
        }
    }
}
