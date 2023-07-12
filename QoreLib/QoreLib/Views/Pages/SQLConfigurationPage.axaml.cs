using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Services;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.Pages;

public partial class SQLConfigurationPage : UserControl
{
    public SQLConfigurationPage()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IDatabaseService, DatabaseService>()
            .BuildServiceProvider();
        var databaseService = serviceProvider.GetService<IDatabaseService>();
        DataContext = new SQLConfigurationPageViewModel(databaseService);
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}