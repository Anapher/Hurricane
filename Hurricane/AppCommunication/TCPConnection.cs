using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Hurricane.AppCommunication
{
    public class TCPConnection
    {
        public event EventHandler Disconnected;

        public StreamProvider StreamProvider { get; set; }
        public TcpClient TcpClient { get; set; }

        public string ID { get; set; }
        public ConnectionDeviceInfo DeviceInfo { get; set; }

        private readonly AppCommunicationSettings _settings;
        public TCPConnection(TcpClient client, AppCommunicationSettings settings)
        {
            StreamProvider = new StreamProvider(client.GetStream());
            client.ReceiveTimeout = 5000;
            _settings = settings;
            TcpClient = client;
        }

        public bool Authenticate()
        {
            string line;
            try
            {
                line = StreamProvider.StreamReader.ReadLine();
            }
            catch (IOException)
            {
                return false;
            }

            if (string.IsNullOrEmpty(line)) return false;

            var match = Regex.Match(line, "^handshake;pass=(?<password>(.*?));id=(?<id>(.*?))$");
            if (!match.Success)
            {
                StreamProvider.SendLine("authentication;invalidhandshake");
                return false;
            }

            if (match.Groups["password"].Value != _settings.Password)
            {
                StreamProvider.SendLine("authentication;wrongpassword");
                return false;
            }

            ID = match.Groups["id"].Value;

            if (_settings.BannedClients.Contains(ID))
            {
                StreamProvider.SendLine("authentication;banned");
                return false;
            }

            StreamProvider.SendLine("authentication;success");

            var deviceInfo = StreamProvider.StreamReader.ReadLine();
            try
            {
                DeviceInfo = ConnectionDeviceInfo.FromString(deviceInfo);
            }
            catch (Exception)
            {
                StreamProvider.SendLine("authentication:invaliddeviceinfo");
                return false;
            }

            StreamProvider.SendLine("authentication;accepted");
            return true;
        }

        public void StartListening()
        {
            TcpClient.ReceiveTimeout = 600000;
            while (true)
            {
                try
                {
                    if (TcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] checkConn = new byte[1];
                        if (TcpClient.Client.Receive(checkConn, SocketFlags.Peek) == 0)
                        {
                            throw new IOException();
                        }
                    }
                    var command = StreamProvider.StreamReader.ReadLine();
                    Commander.ExecuteCommand(command, StreamProvider);
                }
                catch (Exception)
                {
                    break;
                }
            }

            if (Disconnected != null) Disconnected(this, EventArgs.Empty);
            if (StreamProvider != null) StreamProvider.Dispose();
            if (TcpClient != null) TcpClient.Close();

        }

        public void Disconnect()
        {
            StreamProvider.Dispose();
            TcpClient.Close();
            TcpClient = null;
            StreamProvider = null;
        }
    }
}
