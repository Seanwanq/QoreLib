using CommunityToolkit.Mvvm.ComponentModel;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class BottomBarViewModel : BarViewModelBase
{
    public BottomBarViewModel()
    {
        IsActive = true;
    }

    [ObservableProperty] private string _greeting = "hello world";
}