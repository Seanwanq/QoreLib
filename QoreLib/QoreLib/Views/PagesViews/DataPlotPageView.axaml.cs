using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.PagesViews;

public partial class DataPlotPageView : UserControl
{
    public DataPlotPageView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}