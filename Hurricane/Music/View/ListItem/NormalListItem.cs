using System;
using System.Windows;
using System.Windows.Media;
using Hurricane.ViewModelBase;

namespace Hurricane.Music.View.ListItem
{
    class NormalListItem : PropertyChangedBase, IListItem
    {
        public GeometryGroup VectorIcon { get; set; }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { SetProperty(value, ref _isPlaying); }
        }

        public string Name { get; private set; }
        public ListItemGroup Group { get; private set; }

        public NormalListItem(string name, ListItemGroup group, ListItemIcon icon) : this(name, group, null)
        {
            GeometryGroup iconVector;
            switch (icon)
            {
                case ListItemIcon.Queue:
                    iconVector = (GeometryGroup)Application.Current.Resources["VectorQueue"];
                    break;
                case ListItemIcon.Charts:
                    iconVector = (GeometryGroup)Application.Current.Resources["VectorCharts"];
                    break;
                case ListItemIcon.History:
                    iconVector = (GeometryGroup)Application.Current.Resources["VectorHistory"];
                    break;
                case ListItemIcon.Favorites:
                    iconVector = (GeometryGroup)Application.Current.Resources["VectorFavorite"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("icon");
            }

            Initalize(name, group, iconVector);
        }

        public NormalListItem(string name, ListItemGroup group, GeometryGroup icon)
        {
            Initalize(name, group, icon);
        }

        protected void Initalize(string name, ListItemGroup group, GeometryGroup icon)
        {
            Name = name;
            Group = group;
            VectorIcon = icon;
        }
    }

    enum ListItemIcon
    {
        Queue,
        Charts,
        History,
        Favorites
    }
}