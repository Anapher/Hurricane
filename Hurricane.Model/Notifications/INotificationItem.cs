using System;

namespace Hurricane.Model.Notifications
{
    public interface INotificationItem
    {
        string Title { get; }
        event EventHandler Close;
    }
}