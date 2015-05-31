using System;

namespace Hurricane.Model.Notifications
{
    public interface IProgressReporter
    {
        event EventHandler<double> ProgressChanged;
        event EventHandler<string> ProgressMessageChanged;
        event EventHandler Finished;
        void Cancel();
    }
}