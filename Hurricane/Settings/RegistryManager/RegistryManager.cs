using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.Settings.RegistryManager
{
    class RegistryManager
    {
        const string standardname = "PlayWithHurricane";

        public List<RegistryContextMenuItem> ContextMenuItems { get; set; }

        public RegistryManager()
        {
            ContextMenuItems = new List<RegistryContextMenuItem>();
            string[] fileextension = new string[] { ".mp3", ".mpeg3", ".wav", ".wave", ".flac", ".fla", ".aac", ".adt", ".adts", ".m2ts", ".mp2", ".3g2", ".3gp2", ".3gp", ".3gpp", ".m4a", ".m4v", ".mp4v", ".mp4", ".mov", ".asf", ".wm", ".wmv", ".wma" };
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location + " \"%1\"";
            string iconpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            foreach (var s in fileextension)
            {
                //#regdisable ContextMenuItems.Add(new RegistryContextMenuItem(s, standardname, System.Windows.Application.Current.FindResource("PlayWithHurricane").ToString(), apppath, iconpath));
            }
        }
    }
}
