using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Hurricane.AppMainWindow.MahAppsExtensions.Dialogs;
using Hurricane.AppMainWindow.WindowSkins;
using Hurricane.Views;
using MahApps.Metro.Controls.Dialogs;
using InputDialog = Hurricane.Views.InputDialog;

namespace Hurricane.AppMainWindow.Messages
{
    public class WindowDialogService
    {
        public MainWindow BaseWindow { get; set; }
        public WindowSkinConfiguration Configuration { get { return BaseWindow.HostedWindow.Configuration; } }


        public WindowDialogService(MainWindow baseWindow)
        {
            BaseWindow = baseWindow;
        }

        public async Task<ProgressDialog> CreateProgressDialog(string title, bool isindeterminate)
        {
            var prg = new ProgressDialog();
            if (Configuration.ShowFullscreenDialogs)
            {
                var progresscontroller = await BaseWindow.ShowProgressAsync(title, string.Empty);
                progresscontroller.SetIndeterminate();
                prg.MessageChanged = ev =>
                    progresscontroller.SetMessage(ev);
                prg.TitleChanged = ev =>
                    progresscontroller.SetTitle(ev);
                prg.ProgressChanged = ev =>
                    progresscontroller.SetProgress(ev);
                prg.CloseRequest = () =>
                 progresscontroller.CloseAsync();
            }
            else
            {
                var progressWindow = new ProgressWindow(title, isindeterminate) { Owner = BaseWindow };
                prg.MessageChanged = ev => progressWindow.SetText(ev);
                prg.TitleChanged = ev => progressWindow.SetTitle(ev);
                prg.ProgressChanged = ev => progressWindow.SetProgress(ev);
                prg.CloseRequest = () => { progressWindow.Close(); return null; };
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => progressWindow.ShowDialog()));

            }
            return prg;
        }

        public async Task<bool> ShowMessage(string message, string title, bool cancancel, DialogMode mode)
        {
            if (Configuration.ShowFullscreenDialogs)
            {
                MessageDialogResult result =
                    await
                        BaseWindow.ShowMessageAsync(title, message,
                            cancancel ? MessageDialogStyle.AffirmativeAndNegative : MessageDialogStyle.Affirmative,
                            new MetroDialogSettings()
                            {
                                AffirmativeButtonText = "OK",
                                NegativeButtonText = Application.Current.Resources["Cancel"].ToString(),
                                AnimateHide = ShowHideAnimation(mode),
                                AnimateShow = ShowShowAnimation(mode),
                                ColorScheme = MetroDialogColorScheme.Theme
                            });
                return result == MessageDialogResult.Affirmative;
            }
            else
            {
                var messageWindow = new MessageWindow(message, title, cancancel) { Owner = BaseWindow };
                var result = false;
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => result = messageWindow.ShowDialog() == true));
                return result;
            }
        }

        public async Task<string> ShowInputDialog(string title, string message, string buttonok, string defaulttext, DialogMode mode)
        {
            if (Configuration.ShowFullscreenDialogs)
            {
                var dialog = new AdvancedInputDialog(BaseWindow,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = buttonok,
                        DefaultText = defaulttext,
                        NegativeButtonText = Application.Current.Resources["Cancel"].ToString(),
                        ColorScheme = MetroDialogColorScheme.Theme,
                        AnimateHide = ShowHideAnimation(mode),
                        AnimateShow = ShowShowAnimation(mode)
                    }) { Title = title, Message = message };

                await BaseWindow.ShowMetroDialogAsync(dialog);
                var result = await dialog.WaitForButtonPressAsync();
                await dialog._WaitForCloseAsync();
                var foo = BaseWindow.HideMetroDialogAsync(dialog);
                return result;
            }
            else
            {
                var inputdialog = new InputDialog(title, message, buttonok, defaulttext) { Owner = BaseWindow };
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => inputdialog.ShowDialog()));
                return inputdialog.ResultText;
            }
        }

        public void ShowDialog(Window dialog)
        {
            dialog.Owner = BaseWindow;
            dialog.WindowStartupLocation =
                Configuration.ShowFullscreenDialogs
                    ? WindowStartupLocation.CenterScreen
                    : WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }

        private bool ShowHideAnimation(DialogMode mode)
        {
            switch (mode)
            {
                case DialogMode.Single:
                    return true;
                case DialogMode.First:
                    return false;
                case DialogMode.Last:
                    return true;
                case DialogMode.Following:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        private bool ShowShowAnimation(DialogMode mode)
        {
            switch (mode)
            {
                case DialogMode.Single:
                    return true;
                case DialogMode.First:
                    return true;
                case DialogMode.Last:
                    return false;
                case DialogMode.Following:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}
