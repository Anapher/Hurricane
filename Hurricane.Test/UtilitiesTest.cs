using System.Collections.Generic;
using Hurricane.Music.Track;
using Hurricane.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void GenreConvertingIsWorking()
        {
            var genres = new Dictionary<string, Genre>
            {
                {"Heavy Metal", Genre.Metal},
                {"Electropop", Genre.ElectropopAndDisco},
                {"Dance", Genre.DanceAndHouse},
                {"Pop/Funk", Genre.Pop},
                {"Hard Rock", Genre.Rock},
                {"Un Garcon", Genre.Other}
            };

            foreach (var genre in genres)
            {
                Assert.AreEqual(genre.Value, PlayableBase.StringToGenre(genre.Key));
            }

            Assert.AreEqual(PlayableBase.GenreToString(Genre.Rock), "Rock");
            Assert.AreEqual(PlayableBase.GenreToString(Genre.EasyListening), "Easy Listening");
            Assert.AreEqual(PlayableBase.GenreToString(Genre.DanceAndHouse), "Dance & House");
        }

        [TestMethod]
        public void GetAbsolutePathTest()
        {
            const string absolutePath = @"C:\Hurricane\Garcon\baguette\asd.mp3";
            const string relativePath = "asd.mp3";
            const string relativePath2 = @"\foo.mp3";
            const string relativePath3 = @"\hello\fromage.mp3";
            const string directory = @"C:\Hurricane\Garcon\baguette";

            Assert.AreEqual(absolutePath, FileSystemHelper.GetAbsolutePath(absolutePath, directory));
            Assert.AreEqual(@"C:\Hurricane\Garcon\baguette\asd.mp3", FileSystemHelper.GetAbsolutePath(relativePath, directory));
            Assert.AreEqual(@"C:\Hurricane\Garcon\baguette\foo.mp3", FileSystemHelper.GetAbsolutePath(relativePath2, directory));
            Assert.AreEqual(@"C:\Hurricane\Garcon\baguette\hello\fromage.mp3", FileSystemHelper.GetAbsolutePath(relativePath3, directory));
        }
    }
}
