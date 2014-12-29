using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hurricane.Utilities;
using System.Security.Permissions;
using System.Security;

namespace Hurricane.Settings.RegistryManager
{
    class RegistryContextMenuItem : ViewModelBase.PropertyChangedBase
    {
        public string Extension { get; set; }

        public bool IsRegistered
        {
            get { return RegistryRegister.CheckIfExtensionExists(Extension, standardname); }
            set
            {
                ToggleRegister(value, true);
                OnPropertyChanged("IsRegistered");
            }
        }

        public void ToggleRegister(bool value, bool checkadmin)
        {
            try
            {
                if (value)
                {
                    RegistryRegister.RegisterExtension(Extension, header, standardname, apppath, iconpath);
                }
                else
                {
                    RegistryRegister.UnregisterExtension(Extension, standardname);
                }
            }
            catch (SecurityException)
            {
                if (checkadmin)
                {
                    var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, string.Format("/registry \"{0}\"", this.Extension))
                    {
                        Verb = "runas" // indicates to elevate privileges
                    };

                    var process = new Process
                    {
                        EnableRaisingEvents = true, // enable WaitForExit()
                        StartInfo = info
                    };

                    process.Start();
                    process.WaitForExit(); // sleep calling process thread until evoked process exit
                }
            }
        }

        protected string standardname;
        protected string header;
        protected string apppath;
        protected string iconpath;

        public RegistryContextMenuItem(string extension, string standardname, string header, string apppath, string iconpath)
        {
            this.Extension = extension;
            this.standardname = standardname;
            this.header = header;
            this.apppath = apppath;
            this.iconpath = iconpath;
        }

        protected bool HavePermissionsOnKey(RegistryPermissionAccess accessLevel, string key)
        {
            try
            {
                RegistryPermission r = new RegistryPermission(accessLevel, key);
                r.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }
    }
}
