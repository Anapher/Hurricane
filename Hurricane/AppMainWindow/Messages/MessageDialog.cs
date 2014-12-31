using System;

namespace Hurricane.AppMainWindow.Messages
{
    public class MessageDialog
    {
        public event EventHandler<bool> Closed;

        public void DialogClosed(bool isOk)
        {
            if (Closed != null) Closed(this, isOk);
        }
    }

    public class MessageDialogStartEventArgs : EventArgs
    {
        public MessageDialog Instance { get; set; }
        public string Message { get; protected set; }
        public string Title { get; protected set; }
        public bool CanCancel { get; protected set; }

        public MessageDialogStartEventArgs(MessageDialog instance, string message, string title, bool cancancel)
        {
            Message = message;
            Title = title;
            CanCancel = cancancel;
            Instance = instance;
        }
    }
}
