using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Hurricane.ViewModel.MainView
{
    public class CollectionView : IViewItem
    {
        public ViewCategorie ViewCategorie { get; } = ViewCategorie.MyMusic;
        public Geometry Icon { get; }
        public string Text { get; } = "Collection";

        public async Task Load()
        {

        }
    }
}