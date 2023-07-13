using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.PagesViews;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        DataContext = new HomePageViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}