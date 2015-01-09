using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hurricane.Views.UserControls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.AppMainWindow.MahAppsExtensions.Dialogs
{
    /// <summary>
    /// Interaction logic for EqualizerDialog.xaml
    /// </summary>
    public partial class EqualizerDialog
    {
        internal EqualizerDialog(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
        }

        internal Task WaitForCloseAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            KeyEventHandler escapeKeyHandler = null;
            EventHandler closeHandler = null;
            EventHandler viewcloseHandler = null;

            Action cleanUpHandlers = () =>
            {
                KeyDown -= escapeKeyHandler;
                ((EqualizerView)Content).WantClose -= closeHandler;
            };
            
            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();
                    tcs.TrySetResult(null);
                }
            };

            closeHandler = (sender, e) =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(null);
            };

            viewcloseHandler = (sender, e) =>
            {
                cleanUpHandlers();
                tcs.TrySetResult(null);
            };

            KeyDown += escapeKeyHandler;
            ((EqualizerView)Content).WantClose += closeHandler;

            return tcs.Task;
        }
    }
}
