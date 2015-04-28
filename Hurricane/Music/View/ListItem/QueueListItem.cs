using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Hurricane.Music.View.ListItem
{
    class QueueListItem : IListItem
    {
        private static GeometryGroup _vectorIcon;
        public GeometryGroup VectorIcon
        {
            get
            {
                return _vectorIcon ?? (_vectorIcon = (GeometryGroup)Application.Current.Resources["VectorQueue"]);
            }
        }

        public bool IsPlaying
        {
            get { return false; }
        }

        public string Name
        {
            get { return Application.Current.Resources["Queue"].ToString(); }
        }

        public ListItemGroup Group
        {
            get { return ListItemGroup.Discover; }
        }

        public QueueManager QueueManager { get; set; }

    }
}