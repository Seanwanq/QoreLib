using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.ControlsViewModels;

namespace QoreLib.Views.Controls;

public partial class TopBar : UserControl
{
    public TopBar()
    {
        DataContext = new TopBarViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}