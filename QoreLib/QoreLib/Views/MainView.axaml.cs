using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using QoreLib.ViewModels;

namespace QoreLib.Views;

public partial class MainView : UserControl
{
    private WindowNotificationManager? _notificationManager;
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        _notificationManager = new WindowNotificationManager(topLevel) { MaxItems = 3 };
        ServiceLocator.Instance.NotificationManager = _notificationManager;
    }
}