using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QoreLib.Models;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.ViewModels.PagesViewModels;

public partial class DataPlotPageViewModel : PageViewModelBase
{
    private static IDatabaseService? _databaseService;

    public DataPlotPageViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
        if (_databaseService != null) _databaseService.DatabaseConnectedChanged += OnDatabaseConnectedChanged;
    }

    [ObservableProperty] private bool _isW01Selected = true;
    [ObservableProperty] private bool _isW12Selected = false;
    [ObservableProperty] private bool _isPageBanned = true;
    [ObservableProperty] private bool _isDataBad = false;
    [ObservableProperty] private int _id = 0;
    [ObservableProperty] private int _displayGroupId;
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private string _displayType;
    [ObservableProperty] private string _displayApp;

    partial void OnIsW01SelectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "w01channel");
    }

    partial void OnIsDataBadChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "databadchannel");
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            DisplayGroupId = _databaseService.GetGroupIdById(Id);
            DisplayName = _databaseService.GetNameById(Id);
            DisplayApp = _databaseService.GetAppAndTypeById(Id)[0];
            DisplayType = _databaseService.GetAppAndTypeById(Id)[1];
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<int>(value), "idchannel");
        }
    }

    [RelayCommand]
    private void GoTo()
    {
        try
        {
            Id = _databaseService.GetIdByGroupIdAndName(DisplayGroupId, DisplayName);
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Notice"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void ClearPoints()
    {
        ChartViewModel.W01Points.Clear();
        ChartViewModel.W12Points.Clear();
    }

    protected override void OnActivated()
    {
        Messenger.Register<DataPlotPageViewModel, ValueChangedMessage<bool>, string>(this, "datagoodchannel",
            ((r, m) => r.IsDataBad = !m.Value));
    }

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        IsPageBanned = !isConnected;
        if (isConnected)
        {
            // Id = _databaseService.GetFirstDataId();
            Id = 905;
            DisplayGroupId = _databaseService.GetGroupIdById(Id);
            DisplayName = _databaseService.GetNameById(Id);
            DisplayApp = _databaseService.GetAppAndTypeById(Id)[0];
            DisplayType = _databaseService.GetAppAndTypeById(Id)[1];
        }
        else
        {
            Id = 0;
        }
    }

    [RelayCommand]
    private void NextId()
    {
        try
        {
            var nextId = _databaseService.GetNextId(Id);
            if (nextId == -1)
            {
                Id = _databaseService.GetLastDataId();
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = "Already at the last plot.",
                        NoteType = NotificationType.Information,
                        Title = "Notice"
                    }), "notificationchannel");
            }
            else
            {
                Id = nextId;
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void PreviousId()
    {
        try
        {
            var previousId = _databaseService.GetPreviousId(Id);
            if (previousId == -2)
            {
                Id = _databaseService.GetFirstDataId();
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = "Already at the first plot.",
                        NoteType = NotificationType.Information,
                        Title = "Notice"
                    }), "notificationchannel");
            }
            else
            {
                Id = previousId;
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void NextUnfilledId()
    {
        try
        {
            var nextUnfilledId = _databaseService.GetNextUnfilledId(Id);
            if (nextUnfilledId == -1)
            {
                Id = _databaseService.GetLastDataId();
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = "There is no unfilled plot after.",
                        NoteType = NotificationType.Information,
                        Title = "Notice"
                    }), "notificationchannel");
            }
            else
            {
                Id = nextUnfilledId;
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void PreviousUnfilledId()
    {
        try
        {
            var previousUnfilledId = _databaseService.GetPreviousId(Id);
            if (previousUnfilledId == -2)
            {
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = "There is no unfilled plot before.",
                        NoteType = NotificationType.Information,
                        Title = "Notice"
                    }), "notificationchannel");
            }
            else
            {
                Id = previousUnfilledId;
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void LastId()
    {
        try
        {
            Id = _databaseService.GetLastDataId();
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = "Already at the last plot.", NoteType = NotificationType.Information, Title = "Notice"
                }), "notificationchannel");
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }

    [RelayCommand]
    private void FirstId()
    {
        try
        {
            Id = _databaseService.GetFirstDataId();
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = "Already at the first plot.",
                    NoteType = NotificationType.Information,
                    Title = "Notice"
                }), "notificationchannel");
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                }), "notificationchannel");
        }
    }
}