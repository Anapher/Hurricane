using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Hurricane.Utilities;

namespace Hurricane.Settings.RegistryManager
{
    class RegistryManager
    {
        const string standardname = "PlayWithHurricane";

        public List<RegistryContextMenuItem> ContextMenuItems { get; set; }

        public RegistryManager()
        {
            /*
            ContextMenuItems = new List<RegistryContextMenuItem>();
            string[] fileextension = new string[] { ".mp3", ".mpeg3", ".wav", ".wave", ".flac", ".fla", ".aac", ".adt", ".adts", ".m2ts", ".mp2", ".3g2", ".3gp2", ".3gp", ".3gpp", ".m4a", ".m4v", ".mp4v", ".mp4", ".mov", ".asf", ".wm", ".wmv", ".wma" };
            string apppath = Assembly.GetExecutingAssembly().Location + " \"%1\"";
            string iconpath = Assembly.GetExecutingAssembly().Location;
            
            foreach (var s in fileextension)
            {
                ContextMenuItems.Add(new RegistryContextMenuItem(s, standardname, Application.Current.FindResource("PlayWithHurricane").ToString(), apppath, iconpath));
            }#registrydisable*/
        }

        protected readonly FileInfo shortcutpath = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "SendTo", "Hurricane.lnk"));
        public bool SendToShortcut
        {
            get
            {
                shortcutpath.Refresh();
                return shortcutpath.Exists;
            }
            set
            {
                if (value)
                {
                    string apppath = Assembly.GetExecutingAssembly().Location;
                    GeneralHelper.CreateShortcut(shortcutpath.FullName, apppath, apppath);
                }
                else
                {
                    if (shortcutpath.Exists) shortcutpath.Delete();
                }
            }
        }
    }
}
