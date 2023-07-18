using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Models;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private static readonly ServiceProvider _serviceProvider = new ServiceCollection().AddSingleton<IDatabaseService, DatabaseService>().BuildServiceProvider();
    private static readonly IDatabaseService? _databaseService = _serviceProvider.GetService<IDatabaseService>();


    private const int _initialPageIndex = 0;
    [ObservableProperty] private int _pageIndex = 0;

    public MainViewModel()
    {
        IsActive = true;
        ServiceLocator.Instance.DatabaseService = _databaseService;
        ServiceLocator.Instance.ServiceProvider = _serviceProvider;
    }

    private static readonly PageViewModelBase[] Pages =
    {
        new HomePageViewModel(),
        new SQLConfigurationPageViewModel(_databaseService),
        new DataPlotPageViewModel(_databaseService),
    };

    [ObservableProperty] private PageViewModelBase _currentPage = Pages[_initialPageIndex];

    private static readonly double _bbHeight = 140;

    [ObservableProperty]
    private GridLength _bottomBarHeight = new GridLength(_bbHeight);

    [ObservableProperty] private bool _isLeftBarOpen = true;

    [ObservableProperty] private bool _isRightBarOpen = true;

    [ObservableProperty] private bool _isBottomBarOpen = true;

    [ObservableProperty] private NotificationModel? _chartNotification = null;

    protected override void OnActivated()
    {
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "leftbarchannel", LeftBarAdjustment);
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "rightbarchannel", RightBarAdjustment);
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "bottombarchannel", BottomBarAdjustment);
        Messenger.Register<MainViewModel, ValueChangedMessage<int>, string>(this, "leftbarbuttonchannel", (recipient, message) => recipient.PageIndex = message.Value);
        Messenger.Register<MainViewModel, ValueChangedMessage<NotificationModel>, string>(this, "chartnotificationchannel",((recipient, message) => recipient.ChartNotification = message.Value));
    }

    private void LeftBarAdjustment(MainViewModel r, ValueChangedMessage<bool> m)
    {
        r.IsLeftBarOpen = m.Value;
    }

    private void RightBarAdjustment(MainViewModel r, ValueChangedMessage<bool> m)
    {
        r.IsRightBarOpen = m.Value;
    }

    private void BottomBarAdjustment(MainViewModel r, ValueChangedMessage<bool> m)
    {
        r.IsBottomBarOpen = m.Value;
    }

    partial void OnIsBottomBarOpenChanged(bool value)
    {
        BottomBarHeight = IsBottomBarOpen ? new GridLength(_bbHeight) : new GridLength(0);
    }

    partial void OnPageIndexChanged(int value)
    {
        CurrentPage = Pages[PageIndex];
    }

    partial void OnChartNotificationChanged(NotificationModel value)
    {
        ServiceLocator.Instance.NotificationManager.Show(new Notification(value.Title, value.Content, value.NoteType));
    }
}