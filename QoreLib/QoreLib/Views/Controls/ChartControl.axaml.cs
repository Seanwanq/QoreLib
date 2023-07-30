using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using QoreLib.Services;
using QoreLib.ViewModels.ControlsViewModels;
using QoreLib.ViewModels.PagesViewModels;

namespace QoreLib.Views.Controls;

public partial class ChartControl : UserControl
{
    public ChartControl()
    {
        InitializeComponent();
        var databaseService = ServiceLocator.Instance.DatabaseService;
        DataContext = new ChartViewModel(databaseService, this);

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