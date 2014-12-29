using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hurricane.AppMainWindow.Messages
{
    public class MessageManager
    {
        public Action<ProgressDialogStartEventArgs> ProgressDialogStart;
        public Action<MessageDialogStartEventArgs> MessageDialogStart;


        public ProgressDialog CreateProgressDialog(string title, bool isindeterminate)
        {
            ProgressDialog prg = new ProgressDialog();
            if (this.ProgressDialogStart != null) ProgressDialogStart.Invoke(new ProgressDialogStartEventArgs(prg, title, isindeterminate));
            return prg;
        }
    }
}
