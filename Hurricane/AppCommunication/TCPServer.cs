using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Hurricane.AppCommunication.CommunicationEventArgs;

namespace Hurricane.AppCommunication
{
    public class TCPServer
    {
        public event EventHandler<TCPConnectionChangedEventArgs> ClientConnected;
        public event EventHandler<TCPConnectionChangedEventArgs> ClientDisconnected;

        private TcpListener _listener;

        private readonly IPEndPoint _connectionEndPoint;
        private readonly AppCommunicationSettings _settings;
        public TCPServer(ushort port, AppCommunicationSettings settings)
        {
            _connectionEndPoint = new IPEndPoint(IPAddress.Any, port);
            _settings = settings;
        }

        public void StartListening()
        {
            _listener = new TcpListener(_connectionEndPoint);
            _listener.Start();

            var t = new Thread(ListenerThread) { IsBackground = true };
            t.Start();
        }

        public void StopListening()
        {
            _listener.Stop();
        }

        private void ListenerThread()
        {
            while (true)
            {
                TcpClient client;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break;
                }

                var t = new Thread(() =>
                {
                    var connection = new TCPConnection(client, _settings);
                    if (connection.Authenticate())
                    {
                        Application.Current.Dispatcher.Invoke(() => OnClientConnected(connection));
                        connection.Disconnected += connection_Disconnected;
                        connection.StartListening();
                    }
                });
                t.Start();
            }
        }

        void connection_Disconnected(object sender, EventArgs e)
        {
            var connection = (TCPConnection) sender;
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => OnClientDisconnected(connection));
        }

        protected void OnClientConnected(TCPConnection connection)
        {
            if (ClientConnected != null) ClientConnected(this, new TCPConnectionChangedEventArgs(connection));
        }

        protected void OnClientDisconnected(TCPConnection connection)
        {
            if (ClientDisconnected != null) ClientDisconnected(this, new TCPConnectionChangedEventArgs(connection));
        }
    }
}