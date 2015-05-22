using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Hurricane.Model;

namespace Hurricane.ViewModel.MainView
{
    class ChartsView : PropertyChangedBase, IViewItem
    {
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public Geometry Icon { get; }
        public string Text { get; }
        public bool IsPlaying { get; set; }
        public Task Load()
        {
            throw new NotImplementedException();
        }
    }
}
