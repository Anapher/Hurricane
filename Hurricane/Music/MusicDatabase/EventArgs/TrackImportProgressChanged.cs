namespace Hurricane.Music.MusicDatabase.EventArgs
{
   public class TrackImportProgressChangedEventArgs : System.EventArgs
    {
        public double Percentage { get;protected set; }
        public int TotalFiles { get; protected set; }
        public int FilesImported { get; protected set; }
        public string CurrentFile { get; protected set; }

        public TrackImportProgressChangedEventArgs(int filesimported, int totalfiles, string currentfile)
        {
            TotalFiles = totalfiles;
            FilesImported = filesimported;
            Percentage = filesimported / (double)totalfiles;
            CurrentFile = currentfile;
        }
    }
}
