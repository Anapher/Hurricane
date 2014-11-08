using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Hurricane.APIService
{
    class Service : IDisposable
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<TcpClient> Clients;
        private bool IsListening;

        public Service()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            Clients = new List<TcpClient>();
        }

        public void Start()
        {
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            IsListening = true;
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (IsListening)
            {
                try
                {
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    Clients.Add(client);
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
                catch { }
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            BinaryReader reader = new BinaryReader(clientStream);
            BinaryWriter writer = new BinaryWriter(clientStream);

            int BufferSize;

            while (true)
            {
                try
                {
                    BufferSize = reader.ReadInt32();
                }
                catch
                {
                    break;
                }

                byte[] buffer = reader.ReadBytes(BufferSize);
                string command = System.Text.Encoding.ASCII.GetString(buffer);
                string response = string.Empty;

                switch (command)
                {
                    case "gh": //GetHandle
                        System.Windows.Application.Current.Dispatcher.Invoke(() => response = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle.ToString());
                        break;
                }

                byte[] responsebuffer = System.Text.Encoding.ASCII.GetBytes(response);
                writer.Write(responsebuffer.Length);
                writer.Write(responsebuffer);
            }

            tcpClient.Close();
        }

        public void Dispose()
        {
            foreach (TcpClient c in Clients)
            {
                c.Close();
            }
            tcpListener.Stop();
            IsListening = false;
        }
    }
}
