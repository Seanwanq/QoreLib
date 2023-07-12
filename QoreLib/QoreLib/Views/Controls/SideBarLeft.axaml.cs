using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class SideBarLeft : UserControl
{
    public SideBarLeft()
    {
        DataContext = new SideBarLeftViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}