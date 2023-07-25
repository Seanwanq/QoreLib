using System;
using System.IO;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QoreLib.Models;
using QoreLib.Services;

namespace QoreLib.ViewModels.PagesViewModels;

public partial class SQLConfigurationPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService? _databaseService;

    public SQLConfigurationPageViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
        if (_databaseService != null) _databaseService.DatabaseConnectedChanged += OnDatabaseConnectedChanged;
    }

    // TODO 删掉上面的变量
    [ObservableProperty] private string _databaseFolderPath = "D:/qorelibdata";
    [ObservableProperty] private string _databaseName = "coredata.db";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(DisconnectDatabaseCommand))]
    private bool _isDatabaseConnected = false;

    [RelayCommand]
    private void Connect()
    {
        try
        {
            if (!IsDatabaseConnected)
            {
                _databaseService?.ConnectToScientificDatabase(DatabaseFolderPath, DatabaseName);
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = $"Successfully connected to database {DatabaseName}.",
                        NoteType = NotificationType.Success,
                        Title = "Success"
                    }), "notificationchannel");
            }
            else
            {
                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = $"Have already connected to database {DatabaseName}.",
                        NoteType = NotificationType.Information,
                        Title = "Notice"
                    }), "notificationchannel");
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
    private void CreateAndConnectDatabase()
    {
        try
        {
            _databaseService?.CreateAndConnectToScientificDatabase(DatabaseFolderPath, DatabaseName);
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = $"Database {DatabaseName} has been created and connected.",
                    NoteType = NotificationType.Success,
                    Title = "Success"
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

    private bool IsDisconnectDatabaseButtonEnabled() => IsDatabaseConnected;

    [RelayCommand(CanExecute = nameof(IsDisconnectDatabaseButtonEnabled))]
    private void DisconnectDatabase()
    {
        try
        {
            _databaseService?.CloseScientificDatabaseConnection();
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = $"Database {DatabaseName} has been \nclosed.",
                    NoteType = NotificationType.Success,
                    Title = "Success"
                }), "notificationchannel");
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = e.Message + "\nYou would better restart the app.",
                    NoteType = NotificationType.Error,
                    Title = "Error"
                }), "notificationchannel");
        }
    }

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        IsDatabaseConnected = isConnected;
    }
}