using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Hurricane.Settings;

namespace Hurricane.Music.API
{
    class TcpServer : IDisposable
    {
        protected TcpListener listener;
        public List<TcpConnection> Connections { get; set; }
        public MusicManager MusicManager { get; set; }
        public Commander Commander { get; set; }
        public bool IsRunning { get; set; }
        public bool GotProblems { get; set; }

        public TcpServer(MusicManager manager)
        {
            this.MusicManager = manager;
            Commander = new Commander(MusicManager);
        }

        public bool StartListening()
        {
            this.Connections = new List<TcpConnection>();
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Any, HurricaneSettings.Instance.Config.ApiPort);
            try
            {
                listener = new TcpListener(ipendpoint);
                listener.Start();
            }
            catch (SocketException)
            {
                GotProblems = true;
                return false;
            }
            GotProblems = false;
            Thread t = new Thread(ListenerThread) { IsBackground = true };
            t.Start();
            IsRunning = true;
            return true;
        }

        public void StopListening()
        {
            listener.Stop();
            IsRunning = false;
        }

        protected void ListenerThread()
        {
            while (true)
            {
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (Exception)
                {
                    break;
                }
                TcpConnection connection = new TcpConnection(client, MusicManager);
                string openline = connection.Reader.ReadLine();
                if (string.IsNullOrEmpty(openline)) continue;
                string[] parameters = openline.Split('|');
                if (parameters.Length < 4 || parameters.Length > 4) { connection.WriteLine("Error: Can't read arguments. Please use \"bool|bool|bool|bool\""); connection.Dispose(); break; }
                bool error = false;
                foreach (var item in parameters)
                {
                    bool temp;
                    if (!bool.TryParse(item, out temp)) { connection.WriteLine("Error: Can't read arguments. Please use \"bool|bool|bool|bool\""); connection.Dispose(); error = true; break; }
                }
                if (error) continue;
                SetParameters(parameters, connection);
                connection.WriteLine("welcome");
                this.Connections.Add(connection);
                Thread t = new Thread(ListenToConnetion) { IsBackground = true };
                t.Start(connection);
            }
        }

        protected void SetParameters(string[] parameters, TcpConnection connection)
        {
            if (bool.Parse(parameters[0])) connection.RegisterPositionChanged();
            if (bool.Parse(parameters[1])) connection.RegisterPlayStateChanged();
            if (bool.Parse(parameters[2])) connection.RegisterTrackChanged();
            if (bool.Parse(parameters[3])) connection.RegisterVolumeChanged();
        }

        protected void ListenToConnetion(object client)
        {
            var tcpclient = (TcpConnection)client;
            using (tcpclient)
            {
                while (true)
                {
                    try
                    {
                        var command = tcpclient.Reader.ReadLine();
                        string result = string.Empty;
                        Application.Current.Dispatcher.Invoke(() => result = Commander.ExecuteCommand(command));
                        if (!string.IsNullOrEmpty(result)) tcpclient.WriteLine(result);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (IsRunning) listener.Stop();
        }
    }
}
