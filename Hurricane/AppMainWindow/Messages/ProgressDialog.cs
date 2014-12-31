using System;
using System.Threading.Tasks;

namespace Hurricane.AppMainWindow.Messages
{
    public class ProgressDialog
    {
        public Action<string> TitleChanged;
        public Action<double> ProgressChanged;
        public Action<string> MessageChanged;
        public Func<Task> CloseRequest;

        public bool IsClosed { get; set; }

        public void SetTitle(string title)
        {
            if (this.TitleChanged != null) TitleChanged.Invoke(title);
        }

        public void SetProgress(double progress)
        {
            if (this.ProgressChanged != null) ProgressChanged.Invoke(progress);
        }

        public void SetMessage(string text)
        {
            if (this.MessageChanged != null) MessageChanged.Invoke(text);
        }

        public async Task Close()
        {
            if (this.CloseRequest != null) { var wait = CloseRequest.Invoke(); if (wait != null)await wait; }
            IsClosed = true;
        }
    }

    public class ProgressDialogStartEventArgs : EventArgs
    {
        public ProgressDialog Instance { get; protected set; }
        public string Title { get; protected set; }
        public bool IsIndeterminate { get; protected set; }

        public ProgressDialogStartEventArgs(ProgressDialog instance, string title, bool isindeterminate)
        {
            this.Instance = instance;
            this.Title = title;
            this.IsIndeterminate = isindeterminate;
        }
    }
}
