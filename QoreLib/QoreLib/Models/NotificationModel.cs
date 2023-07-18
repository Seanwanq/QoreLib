using Avalonia.Controls.Notifications;

namespace QoreLib.Models;

public class NotificationModel
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationType NoteType { get; set; }
}