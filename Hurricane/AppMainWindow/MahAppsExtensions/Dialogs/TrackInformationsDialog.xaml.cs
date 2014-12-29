using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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

namespace Hurricane.AppMainWindow.MahAppsExtensions.Dialogs
{
    public partial class TrackInformationsDialog : BaseMetroDialog
    {

        protected Views.TrackInformationsView view;
        internal TrackInformationsDialog(MetroWindow parentWindow, Music.Track track, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            view = new Views.TrackInformationsView(track);
            view.Width = 600;
            view.Height = 500;
            this.gridContent.Children.Add(view);
        }

        internal Task WaitForCloseAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            KeyEventHandler escapeKeyHandler = null;
            RoutedEventHandler closeHandler = null;
            EventHandler viewcloseHandler = null;

            Action cleanUpHandlers = () =>
            {
                this.KeyDown -= escapeKeyHandler;
                this.closebutton.Click -= closeHandler;
                this.view.CloseRequest -= viewcloseHandler;
                this.view.Dispose();
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

            this.KeyDown += escapeKeyHandler;
            this.closebutton.Click += closeHandler;
            this.view.CloseRequest += viewcloseHandler;

            return tcs.Task;
        }
    }
}
