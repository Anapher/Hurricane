using System;

namespace Hurricane.Music.CustomEventArgs
{
   public class TrackImportProgressChangedEventArgs : EventArgs
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
