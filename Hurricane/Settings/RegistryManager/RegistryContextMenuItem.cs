using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (value)
                {
                    RegistryRegister.RegisterExtension(Extension, header, standardname, apppath, iconpath);
                }
                else
                {
                    RegistryRegister.UnregisterExtension(Extension, standardname);
                }
                OnPropertyChanged("IsRegistered");
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
    }
}
