using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class BottomBar : UserControl
{
    public BottomBar()
    {
        DataContext = new BottomBarViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}