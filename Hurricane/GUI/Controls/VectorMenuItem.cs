using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hurricane.GUI.Controls
{
    public class VectorMenuItem : MenuItem
    {
        public static readonly DependencyProperty VectorGraphicProperty = DependencyProperty.Register(
            "VectorGraphic", typeof(Geometry), typeof(VectorMenuItem), new PropertyMetadata(default(Geometry)));

        public Geometry VectorGraphic
        {
            get { return (Geometry) GetValue(VectorGraphicProperty); }
            set { SetValue(VectorGraphicProperty, value); }
        }

        public VectorMenuItem()
        {
            Style = Application.Current.Resources["ImageMenuItemStyle"] as Style;
        }
    }
}
