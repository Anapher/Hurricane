using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.AppMainWindow.Messages
{
    public class MessageDialog
    {
        public event EventHandler<bool> Closed;

        public void DialogClosed(bool IsOk)
        {
            if (Closed != null) Closed(this, IsOk);
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
            this.Message = message;
            this.Title = title;
            this.CanCancel = cancancel;
            this.Instance = instance;
        }
    }
}
