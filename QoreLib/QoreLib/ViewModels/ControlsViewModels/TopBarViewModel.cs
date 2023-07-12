using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class TopBarViewModel : BarViewModelBase
{
    public TopBarViewModel()
    {
        IsActive = true;
    }

    [ObservableProperty] private string _topBarBackground = "Transparent";
    
    [ObservableProperty] private bool _isLeftBarOpen = true;

    [ObservableProperty] private bool _isRightBarOpen = true;

    [ObservableProperty] private bool _isBottomBarOpen = true;

    partial void OnIsLeftBarOpenChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "leftbarchannel");
    }

    partial void OnIsRightBarOpenChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "rightbarchannel");
    }

    partial void OnIsBottomBarOpenChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "bottombarchannel");
    }
}