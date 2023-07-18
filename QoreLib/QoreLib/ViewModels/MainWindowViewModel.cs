using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace QoreLib.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        IsActive = true;
    }

    [ObservableProperty] private string _title = "QoreLib";

    [ObservableProperty] 
    private Color _tintColor = Colors.White;
}