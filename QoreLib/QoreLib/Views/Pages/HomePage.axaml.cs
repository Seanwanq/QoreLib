using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.Pages;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        DataContext = new HomePageViewModel();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}