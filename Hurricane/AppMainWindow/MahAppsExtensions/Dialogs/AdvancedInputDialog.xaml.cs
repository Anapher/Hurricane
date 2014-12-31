using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hurricane.AppMainWindow.MahAppsExtensions.Dialogs
{
    public partial class AdvancedInputDialog
    {
        internal AdvancedInputDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }
        internal AdvancedInputDialog(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            PART_AffirmativeButton.Content = settings.AffirmativeButtonText;
            PART_NegativeButton.Content = settings.NegativeButtonText;
            Input = settings.DefaultText;
        }

        internal Task<string> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Focus();
                PART_TextBox.Focus();
            }));

            var tcs = new TaskCompletionSource<string>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                PART_TextBox.KeyDown -= affirmativeKeyHandler;

                KeyDown -= escapeKeyHandler;

                PART_NegativeButton.Click -= negativeHandler;
                PART_AffirmativeButton.Click -= affirmativeHandler;

                PART_NegativeButton.KeyDown -= negativeKeyHandler;
                PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(Input);
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(Input);

                e.Handled = true;
            };

            PART_NegativeButton.KeyDown += negativeKeyHandler;
            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            PART_TextBox.KeyDown += affirmativeKeyHandler;

            KeyDown += escapeKeyHandler;

            PART_NegativeButton.Click += negativeHandler;
            PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            PART_TextBox.SelectAll();
            switch (DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    PART_NegativeButton.Style = FindResource("HighlightedSquareButtonStyle") as Style;
                    PART_TextBox.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(string), typeof(InputDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(InputDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(InputDialog), new PropertyMetadata("Cancel"));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public string AffirmativeButtonText
        {
            get { return (string)GetValue(AffirmativeButtonTextProperty); }
            set { SetValue(AffirmativeButtonTextProperty, value); }
        }

        public string NegativeButtonText
        {
            get { return (string)GetValue(NegativeButtonTextProperty); }
            set { SetValue(NegativeButtonTextProperty, value); }
        }
    }
}
