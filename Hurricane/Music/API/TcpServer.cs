using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Data;

namespace Hurricane.Music.API
{
    class TcpServer : IDisposable
    {
        protected TcpListener listener;
        public List<TcpConnection> Connections { get; set; }
        public Music.MusicManager MusicManager { get; set; }
        public Commander Commander { get; set; }
        public bool IsRunning { get; set; }
        public bool GotProblems { get; set; }

        public TcpServer(Music.MusicManager manager)
        {
            this.MusicManager = manager;
            Commander = new Commander(MusicManager);
        }

        public bool StartListening()
        {
            this.Connections = new List<TcpConnection>();
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Any, Settings.HurricaneSettings.Instance.Config.ApiPort);
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
            System.Threading.Thread t = new System.Threading.Thread(ListenerThread);
            t.IsBackground = true;
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
                connection.WriteLine("go");
                string openline = connection.Reader.ReadLine();
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
                System.Threading.Thread t = new System.Threading.Thread(ListenToConnetion);
                t.IsBackground = true;
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
                        System.Windows.Application.Current.Dispatcher.Invoke(() => result = Commander.ExecuteCommand(command));
                        tcpclient.WriteLine(result);
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
