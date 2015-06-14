using System;
using Hurricane.Model.Music.Playlist;

namespace Hurricane.Model.Data
{
    [Serializable]
    public class UserData
    {
        public UserData()
        {
            History = new History();
        }

        public History History { get; set; }
    }
}