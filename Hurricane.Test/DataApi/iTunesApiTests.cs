using System.Globalization;
using System.Threading.Tasks;
using Hurricane.Model.DataApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hurricane.Test.DataApi
{
    [TestClass()]
    // ReSharper disable once InconsistentNaming
    public class iTunesApiTests
    {
        [TestMethod()]
        public async Task GetTop100Test()
        {
            var results = await iTunesApi.GetTop100(new CultureInfo("de-DE"));
            Assert.AreEqual(results.Count, 100);

            var results2 = await iTunesApi.GetTop100(new CultureInfo("gl-ES"));
            Assert.AreEqual(results2.Count, 100);
        }
    }
}