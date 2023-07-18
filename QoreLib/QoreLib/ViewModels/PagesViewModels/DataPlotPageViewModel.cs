using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<int>(value), "idchannel");
    }

    [RelayCommand]
    void ClearPoints()
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
            Id = 911;
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
                // TODO Send At Last Notification
                Id = _databaseService.GetLastDataId();
            }
            else
            {
                Id = nextId;
            }
        }
        catch (Exception e)
        {
            // TODO Send Error Notification
        }
    }
}