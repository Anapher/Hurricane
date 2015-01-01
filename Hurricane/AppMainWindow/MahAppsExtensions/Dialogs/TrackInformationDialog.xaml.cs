using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hurricane.Music;
using Hurricane.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.AppMainWindow.MahAppsExtensions.Dialogs
{
    public partial class TrackInformationDialog : BaseMetroDialog
    {

        protected TrackInformationView view;
        internal TrackInformationDialog(MetroWindow parentWindow, Track track, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            view = new TrackInformationView(track) { Width = 600, Height = 500 };
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
                KeyDown -= escapeKeyHandler;
                closebutton.Click -= closeHandler;
                view.CloseRequest -= viewcloseHandler;
                view.Dispose();
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
            closebutton.Click += closeHandler;
            view.CloseRequest += viewcloseHandler;

            return tcs.Task;
        }
    }
}
