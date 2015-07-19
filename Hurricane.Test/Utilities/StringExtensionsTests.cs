using Hurricane.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Hurricane.Test.Utilities
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void FixJsonStringTest()
        {
            var testString = "{\r\n\t\"Name\" : \"Garcon\",\r\n\t\"ShowNotChange\" : \"\",\r\n\t\"Similar\" : \"\\n\\t  \",\r\n\t\"Tags\" : \"\\n      \"\r\n}";
            var workingString = "{\r\n\t\"Name\" : \"Garcon\",\r\n\t\"ShowNotChange\" : \"\",\r\n\t\"Similar\" : {\r\n\t\tProperty1 : \"asdasd\",\r\n\t\tProperty2 : \"adasd\"\r\n\t},\r\n\t\"Tags\" : {\r\n\t\tTag1 : \"hello\",\r\n\t\tTag2 : \"wtf\"\r\n\t}\r\n}";

            var object1 = JsonConvert.DeserializeObject<RootObject>(StringExtensions.FixJsonString(testString));
            var object2 = JsonConvert.DeserializeObject<RootObject>(StringExtensions.FixJsonString(workingString));

            Assert.AreEqual(object2.Tags.Tag1, "hello");
            Assert.AreEqual(object2.Similar.Property1, "asdasd");
            Assert.AreEqual(object1.Name, "Garcon");
            Assert.IsNull(object1.Tags);
        }

        public class Similar
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        public class Tags
        {
            public string Tag1 { get; set; }
            public string Tag2 { get; set; }
        }

        public class RootObject
        {
            public string Name { get; set; }
            public string ShowNotChange { get; set; }
            public Similar Similar { get; set; }
            public Tags Tags { get; set; }
        }
    }
}