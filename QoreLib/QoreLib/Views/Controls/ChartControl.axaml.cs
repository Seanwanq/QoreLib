using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.Controls;

public partial class ChartControl : UserControl
{
    private static readonly ServiceProvider _serviceProvider = new ServiceCollection().AddSingleton<IDatabaseService, DatabaseService>().BuildServiceProvider();
    private static readonly IDatabaseService? _databaseService = _serviceProvider.GetService<IDatabaseService>();
    public ChartControl()
    {
        DataContext = new ChartViewModel(_databaseService);
        InitializeComponent();
    }

    private void ChartPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        ChartViewModel.AddPoint(this, sender, e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}