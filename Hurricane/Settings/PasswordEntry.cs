using System;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using Exceptionless.Json;

namespace Hurricane.Settings
{
    public class PasswordEntry
    {
        public string Id { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Field1 { get; set; }
        [XmlIgnore, JsonIgnore]
        public string Field2 { get; set; }

        [XmlElement(ElementName = "Field1")]
        public string Field12Serialize
        {
            get { return EncryptString(Field1); }
            set { Field1 = DecryptString(value); }
        }

        [XmlElement(ElementName = "Field2")]
        public string Field22Serialize
        {
            get { return EncryptString(Field2); }
            set { Field2 = DecryptString(value); }
        }

        private string EncryptString(string str)
        {
            return
                Convert.ToBase64String(Encoding.BigEndianUnicode.GetBytes(Rot17.Encrypt(str)))
                    .ToCharArray()
                    .Select(x => String.Format("{0:X}", (int) x))
                    .Aggregate(new StringBuilder(), (x, y) => x.Append(y))
                    .ToString();
        }

        private string DecryptString(string str)
        {
            return
                Rot17.Decrypt(
                    (Encoding.BigEndianUnicode.GetString(
                        Convert.FromBase64String(
                            Enumerable.Range(0, str.Length/2)
                                .Select(i => str.Substring(i*2, 2))
                                .Select(x => (char) Convert.ToInt32(x, 16))
                                .Aggregate(new StringBuilder(), (x, y) => x.Append(y))
                                .ToString()))));
        }
    }
}