using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Hurricane.ViewModel.MainView.Base;

namespace Hurricane.ViewModel.MainView
{
    public class QueueView : SideListItem
    {
        public override ViewCategorie ViewCategorie { get; } = ViewCategorie.Discover;
        public override Geometry Icon { get; } = (Geometry)Application.Current.Resources["VectorQueue"];
        public override string Text => Application.Current.Resources["Queue"].ToString();
        protected async override Task Load()
        {
            
        }
    }
}