using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hurricane.Model.Notifications
{
    public class ProgressNotification : INotificationItem, INotifyPropertyChanged
    {
        private double _currentProgress;
        private string _message;
        private readonly IProgressReporter _progressReporter;

        public ProgressNotification(string title, IProgressReporter progressReporter)
        {
            Title = title;

            _progressReporter = progressReporter;
            progressReporter.ProgressMessageChanged += ProgressReporter_ProgressMessageChanged;
            progressReporter.ProgressChanged += ProgressReporter_ProgressChanged;
            progressReporter.Finished += ProgressReporter_Finished;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Close;

        public string Title { get; }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                if (!_currentProgress.Equals(value))
                {
                    _currentProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Cancel()
        {
            _progressReporter.Cancel();
        }

        private void ProgressReporter_Finished(object sender, EventArgs e)
        {
            Close?.Invoke(this, EventArgs.Empty);
        }

        private void ProgressReporter_ProgressChanged(object sender, double e)
        {
            CurrentProgress = e;
        }

        private void ProgressReporter_ProgressMessageChanged(object sender, string e)
        {
            Message = e;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}