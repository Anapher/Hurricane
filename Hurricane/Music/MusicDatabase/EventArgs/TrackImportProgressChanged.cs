using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Music
{
   public class TrackImportProgressChangedEventArgs : EventArgs
    {
        public double Percentage { get;protected set; }
        public int TotalFiles { get; protected set; }
        public int FilesImported { get; protected set; }
        public string CurrentFile { get; protected set; }

        public TrackImportProgressChangedEventArgs(int filesimported, int totalfiles, string currentfile)
        {
            this.TotalFiles = totalfiles;
            this.FilesImported = filesimported;
            this.Percentage = (double)filesimported / (double)totalfiles;
            this.CurrentFile = currentfile;
        }
    }
}
