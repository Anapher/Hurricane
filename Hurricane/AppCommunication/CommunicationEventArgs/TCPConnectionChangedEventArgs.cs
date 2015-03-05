using System;

namespace Hurricane.AppCommunication.CommunicationEventArgs
{
    public class TCPConnectionChangedEventArgs : EventArgs
    {
        public TCPConnection Connection { get; set; }

        public TCPConnectionChangedEventArgs(TCPConnection connection)
        {
            Connection = connection;
        }
    }
}