using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hurricane.Music.Playlist;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.Music
{
    [TestClass]
    public class ImportTests
    {
        private readonly List<string> _testFiles;
        private readonly string _simplePlaylist;
        private readonly string _advancedPlaylist;
        private readonly DirectoryInfo _testDirectory;

        public ImportTests()
        {
            _testDirectory = new DirectoryInfo("TestFiles");
            _testFiles = new List<string>(_testDirectory.GetFiles().Select(x => x.FullName)) {"invalidFileASDASASDSD.mp3"};
            _simplePlaylist = Path.Combine(_testDirectory.FullName, "simplePlaylist.m3u");
            _advancedPlaylist = Path.Combine(_testDirectory.FullName, "advancedPlaylist.m3u");
        }

        [TestMethod]
        public void ImportLocalTracks()
        {
            var playlist = new NormalPlaylist();
            var task = playlist.AddFiles(_testFiles);
            task.Wait();
            
            Assert.AreEqual(2, playlist.Tracks.Count);
            var authenticationCodeList = new List<long>();

            foreach (var track in playlist.Tracks)
            {
                if(authenticationCodeList.Contains((track.AuthenticationCode)))
                    Assert.Fail("Same AuthenticationCodes");
                authenticationCodeList.Add(track.AuthenticationCode);
            }
        }

        [TestMethod]
        public void ImportAdvancedM3UPlaylist()
        {
            using (var advancedPlaylistReader = new StreamReader(_advancedPlaylist))
            {
                Assert.IsTrue(ImportM3UPlaylist.IsSupported(advancedPlaylistReader));
            }

            using (var advancedPlaylistReader = new StreamReader(_advancedPlaylist))
            {
                var playlist =
                    ImportM3UPlaylist.Import(_testDirectory.FullName, advancedPlaylistReader);
                Assert.AreEqual(4, playlist.Tracks.Count);
            }
        }

        [TestMethod]
        public void ImportSimpleM3UPlaylist()
        {
            using (var simplePlaylistReader = new StreamReader(_simplePlaylist))
            {
                Assert.IsTrue(ImportM3UPlaylist.IsSupported(simplePlaylistReader));
            }

            using (var simplePlaylistReader = new StreamReader(_simplePlaylist))
            {
                var playlist =
                    ImportM3UPlaylist.Import(_testDirectory.FullName, simplePlaylistReader);
                Assert.AreEqual(4, playlist.Tracks.Count);
            }
        }
    }
}
