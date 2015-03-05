using System.Collections.Generic;

namespace Hurricane.AppCommunication
{
    public class AppCommunicationSettings
    {
        public ushort Port { get; set; }
        public string Password { get; set; }
        public List<string> BannedClients { get; set; }
        public bool IsEnabled { get; set; }

        public AppCommunicationSettings()
        {
            BannedClients = new List<string>();
            SetStandard();
        }

        public void SetStandard()
        {
            Port = 10898; //10.08.1998
            Password = Utilities.PasswordGenerator.GetSystemSpecificHandshake();
        }
    }
}
