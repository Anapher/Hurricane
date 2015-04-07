using System;
using System.Text;
using Hurricane.Utilities;

namespace Hurricane.Music.Proxy
{
    class ProxyEntry
    {
        public int Port { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }

        public int Working { get; set; }
        public int Speed { get; set; }
        public double ResponseTime { get; set; }

        private string _decodedIp;
        public string DecodeIp()
        {
            return _decodedIp ?? (_decodedIp = Encoding.UTF8.GetString(Convert.FromBase64String(GeneralHelper.ROT13(Ip))));
        }
    }
}
