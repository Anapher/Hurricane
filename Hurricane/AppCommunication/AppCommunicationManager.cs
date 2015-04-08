using System.Collections.ObjectModel;
using Hurricane.AppCommunication.CommunicationEventArgs;
using Hurricane.ViewModelBase;

namespace Hurricane.AppCommunication
{
    public class AppCommunicationManager : PropertyChangedBase
    {
        public TCPServer Server { get; set; }
        public ObservableCollection<TCPConnection> ConnectedClients { get; set; }
        public AppCommunicationSettings Settings { get; set; }

        public AppCommunicationManager(AppCommunicationSettings settings)
        {   
            ConnectedClients = new ObservableCollection<TCPConnection>();
            Settings = settings;
        }

        public void Start()
        {
            if (IsRunning) return;
            Server = new TCPServer(Settings.Port, Settings);
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;

            Server.StartListening();
            IsRunning = true;
        }

        void Server_ClientDisconnected(object sender, TCPConnectionChangedEventArgs e)
        {
          ConnectedClients.Remove(e.Connection);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            Server.StopListening();
            foreach (var connectedClient in ConnectedClients)
            {
                connectedClient.Disconnect();
            }
            IsRunning = false;
        }

        void Server_ClientConnected(object sender, TCPConnectionChangedEventArgs e)
        {
            ConnectedClients.Add(e.Connection);
        }

        #region Properties

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                SetProperty(value, ref _isRunning);
            }
        }

        #endregion
    }
}