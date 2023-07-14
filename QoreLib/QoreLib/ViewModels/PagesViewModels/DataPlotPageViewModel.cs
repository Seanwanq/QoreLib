using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.ViewModels.PagesViewModels;

public partial class DataPlotPageViewModel : PageViewModelBase
{
    private readonly IDatabaseService? _databaseService;
    public DataPlotPageViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
    }

    [ObservableProperty] private bool _isW01Selected = true;

    [ObservableProperty] private bool _isW12Selected = false;

    partial void OnIsW01SelectedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "w01channel");
    }

    [RelayCommand]
    void ClearPoints()
    {
        ChartViewModel.W01Points.Clear();
        ChartViewModel.W12Points.Clear();
    }

}