namespace Hurricane.Model.Settings
{
    public class SettingsManager
    {
        private static SettingsManager _instance;

        private SettingsManager()
        {

        }

        public static SettingsManager Current => _instance ?? (_instance = new SettingsManager());

        public string SoundOutId { get; set; }
    }
}