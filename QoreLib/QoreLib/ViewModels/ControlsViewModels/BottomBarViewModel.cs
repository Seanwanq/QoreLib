using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using QoreLib.Services;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class BottomBarViewModel : BarViewModelBase
{
    private readonly IDatabaseService? _databaseService;

    [ObservableProperty] private string _statusColor = "Red";
    [ObservableProperty] private string _statusText = "Database disconnected";
    
    public BottomBarViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
        if (_databaseService != null)
            _databaseService.DatabaseConnectedChanged += OnDatabaseConnectedChanged;
    }

    [ObservableProperty] private string _greeting = "hello world";

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        if (isConnected)
        {
            StatusColor = "SpringGreen";
            StatusText = "Database connected";
        }
        else if(!isConnected)
        {
            StatusColor = "Red";
            StatusText = "Database disconnected";
        }
    }
}