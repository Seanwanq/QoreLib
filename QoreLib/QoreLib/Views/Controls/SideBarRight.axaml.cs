using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class SideBarRight : UserControl
{
    public SideBarRight()
    {
        DataContext = new SideBarRightViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}