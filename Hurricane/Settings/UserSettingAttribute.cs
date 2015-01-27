using System;

namespace Hurricane.Settings
{
    public class UserSettingAttribute : Attribute
    {
        public bool NotifyPropertyChanged { get; set; }
        public Action PropertyChangedCallback { get; set; }
    }
}