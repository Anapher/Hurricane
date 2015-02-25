using System.Windows;
using Hurricane.Designer.Data;

namespace Hurricane.Designer.Pages
{
    /// <summary>
    /// Interaction logic for ThemeEditControl.xaml
    /// </summary>
    public partial class ThemeEditControl
    {

        public static readonly DependencyProperty ThemeDataProperty = DependencyProperty.Register(
            "ThemeData", typeof (DataThemeBase), typeof (ThemeEditControl), new PropertyMetadata(default(DataThemeBase)));

        public DataThemeBase ThemeData
        {
            get { return (DataThemeBase) GetValue(ThemeDataProperty); }
            set { SetValue(ThemeDataProperty, value); }
        }
        
        public ThemeEditControl()
        {
            InitializeComponent();
        }
    }
}
