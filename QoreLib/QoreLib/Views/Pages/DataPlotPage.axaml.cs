using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.Pages;

public partial class DataPlotPage : UserControl
{
    public DataPlotPage()
    {
        DataContext = new DataPlotPageViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}