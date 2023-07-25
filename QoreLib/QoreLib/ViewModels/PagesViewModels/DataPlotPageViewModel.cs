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
    public static int ExposedId;
    [ObservableProperty] private int _displayGroupId;
    [ObservableProperty] private string _displayName;
    [ObservableProperty] private string _displayType;
    [ObservableProperty] private string _displayApp;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CheckOKCommand))]
    private bool _isFilled = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearPointsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckOKCommand))]
    private bool _isEditable = true;

    [ObservableProperty] private bool _isDataUseful = true;
    [ObservableProperty] private bool _isPlotSheltered = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    [NotifyCanExecuteChangedFor(nameof(CheckOKCommand))]
    private bool _isOK = false;

    partial void OnIsW01SelectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "w01channel");
    }

    partial void OnIsDataBadChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "databadchannel");
        IsPlotSheltered = IsDataBad || (!IsEditable);
        IsEditable = (!IsFilled) && (!IsDataBad);
    }

    partial void OnIsEditableChanged(bool value)
    {
        IsPlotSheltered = IsDataBad || (!IsEditable);
    }

    partial void OnIdChanged(int value)
    {
        if (value > 0)
        {
            ExposedId = Id;
            DisplayGroupId = _databaseService!.GetSpectrumDataGroupIdById(Id);
            DisplayName = _databaseService.GetSpectrumDataNameById(Id);
            DisplayApp = _databaseService.GetSpectrumDataAppAndTypeById(Id)["APP"];
            DisplayType = _databaseService.GetSpectrumDataAppAndTypeById(Id)["Type"];
            IsFilled = _databaseService.GetSpectrumDataIsFilledById(Id);
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<int>(value), "idchannel");
            IsEditable = (!IsFilled) && (!IsDataBad);
            if (IsFilled)
            {
                IsDataUseful = _databaseService.GetSpectrumDataIsUsefulAndIsWrongDataById(Id)["IsUseful"];
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(IsDataUseful), "isusefulchannel");
                IsDataBad = _databaseService.GetSpectrumDataIsUsefulAndIsWrongDataById(Id)["IsWrongData"];
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(IsDataBad), "databadchannel");
            }
            else
            {
                IsDataUseful = true;
            }
            
            IsW01Selected = true;
        }
    }

    [RelayCommand]
    private void GoTo()
    {
        try
        {
            Id = _databaseService.GetSpectrumDataIdByGroupIdAndName(DisplayGroupId, DisplayName);
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

    private bool IsClearPointsButtonEnabled() => IsEditable;

    [RelayCommand(CanExecute = nameof(IsClearPointsButtonEnabled))]
    private void ClearPoints()
    {
        ChartViewModel.ClearPoints();
    }

    // private bool IsCheckOkButtonEnabled() => (!IsOK && IsEditable);
    private bool IsCheckOkButtonEnabled()
    {
        if (IsFilled)
        {
            return !IsOK && IsEditable;
        }
        else
        {
            return !IsOK;
        }
    }

    [RelayCommand(CanExecute = nameof(IsCheckOkButtonEnabled))]
    private void CheckOK()
    {
        ChartViewModel.OKCommand();
        IsOK = true;
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(IsOK), "isplotokreceivechannel");
    }

    private bool IsSubmitButtonEnabled() => IsOK;

    [RelayCommand(CanExecute = nameof(IsSubmitButtonEnabled))]
    private void Submit()
    {
        ChartViewModel.SubmitCommand();
        NextUnfilledId();
    }

    protected override void OnActivated()
    {
        Messenger.Register<DataPlotPageViewModel, ValueChangedMessage<bool>, string>(this, "datagoodchannel",
            ((r, m) => r.IsDataBad = !m.Value));
        Messenger.Register<DataPlotPageViewModel, ValueChangedMessage<bool>, string>(this, "isplotoksendchannel",
            (r, m) => r.IsOK = m.Value);
    }

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        IsPageBanned = !isConnected;
        if (isConnected)
        {
            Id = _databaseService.GetSpectrumDataFirstDataId();
            // Id = 906;
            DisplayGroupId = _databaseService.GetSpectrumDataGroupIdById(Id);
            DisplayName = _databaseService.GetSpectrumDataNameById(Id);
            DisplayApp = _databaseService.GetSpectrumDataAppAndTypeById(Id)["APP"];
            DisplayType = _databaseService.GetSpectrumDataAppAndTypeById(Id)["Type"];
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
            var nextId = _databaseService.GetSpectrumDataNextId(Id);
            if (nextId == -1)
            {
                Id = _databaseService.GetSpectrumDataLastDataId();
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
            var previousId = _databaseService.GetSpectrumDataPreviousId(Id);
            if (previousId == -2)
            {
                Id = _databaseService.GetSpectrumDataFirstDataId();
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
            var nextUnfilledId = _databaseService.GetSpectrumDataNextUnfilledId(Id);
            if (nextUnfilledId == -1)
            {
                Id = _databaseService.GetSpectrumDataLastDataId();
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
            var previousUnfilledId = _databaseService.GetSpectrumDataPreviousId(Id);
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
            Id = _databaseService.GetSpectrumDataLastDataId();
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
            Id = _databaseService.GetSpectrumDataFirstDataId();
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

    partial void OnIsDataUsefulChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "isusefulchannel");
    }
}