using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Hurricane.ViewModelBase;

namespace Hurricane.Settings.RegistryManager
{
    class RegistryContextMenuItem : PropertyChangedBase
    {
        public string Extension { get; set; }

        public bool IsRegistered
        {
            get { return RegistryRegister.CheckIfExtensionExists(Extension, _standardname); }
            set
            {
                ToggleRegister(value, true);
                OnPropertyChanged();
            }
        }

        public void ToggleRegister(bool value, bool checkadmin)
        {
            try
            {
                if (value)
                {
                    RegistryRegister.RegisterExtension(Extension, _header, _standardname, _apppath, _iconpath);
                }
                else
                {
                    RegistryRegister.UnregisterExtension(Extension, _standardname);
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

        protected string _standardname;
        protected string _header;
        protected string _apppath;
        protected string _iconpath;

        public RegistryContextMenuItem(string extension, string standardname, string header, string apppath, string iconpath)
        {
            this.Extension = extension;
            this._standardname = standardname;
            this._header = header;
            this._apppath = apppath;
            this._iconpath = iconpath;
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
