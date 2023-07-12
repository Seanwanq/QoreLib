using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class DataPopUp : UserControl
{
    public DataPopUp()
    {
        DataContext = new DataPopUpViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}