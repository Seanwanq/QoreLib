using System.Runtime.InteropServices;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace QoreLib.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        IsActive = true;
    }

    private static readonly double _bbHeight = 140;

    [ObservableProperty]
    private GridLength _bottomBarHeight = new GridLength(_bbHeight);

    [ObservableProperty] private bool _isLeftBarOpen = true;

    [ObservableProperty] private bool _isRightBarOpen = true;

    [ObservableProperty] private bool _isBottomBarOpen = true;

    protected override void OnActivated()
    {
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "leftbarchannel", LeftBarAdjustment);
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "rightbarchannel", RightBarAdjustment);
        Messenger.Register<MainViewModel, ValueChangedMessage<bool>, string>(this, "bottombarchannel", BottomBarAdjustment);
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

}