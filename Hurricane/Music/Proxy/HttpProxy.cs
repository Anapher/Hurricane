using System.Net;

namespace Hurricane.Music.Proxy
{
    public class HttpProxy
    {
        public string Ip { get; private set; }
        public int Port { get; private set; }

        public WebProxy ToWebProxy()
        {
            return new WebProxy(Ip, Port);
        }

        public HttpProxy(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Ip, Port);
        }
    }
}
