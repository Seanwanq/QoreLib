using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using QoreLib.Services;
using QoreLib.Views.Controls;
using SkiaSharp;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class ChartViewModel : ViewModelBase
{
    private readonly IDatabaseService? _databaseService;

    public ChartViewModel(IDatabaseService? databaseService)
    {
        IsActive = true;
        _databaseService = databaseService;
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = new List<ObservablePoint>
                {
                    new ObservablePoint(0, 4), new ObservablePoint(1, 3), new ObservablePoint(3, 8),
                },
                Fill = null,
                GeometrySize = 5,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 1,
                Stroke = new SolidColorPaint { Color = SKColors.DodgerBlue, StrokeThickness = 1 }
            },
            new ScatterSeries<ObservablePoint>
            {
                Values = W01Points, GeometrySize = 10, Fill = new SolidColorPaint(SKColors.LimeGreen)
            },
            new ScatterSeries<ObservablePoint>
            {
                Values = W12Points, GeometrySize = 10, Fill = new SolidColorPaint(SKColors.BlueViolet)
            }
        };
    }

    public static ObservableCollection<ObservablePoint> W01Points { get; set; } =
        new ObservableCollection<ObservablePoint>();

    public static ObservableCollection<ObservablePoint> W12Points { get; set; } =
        new ObservableCollection<ObservablePoint>();

    public ISeries[] Series { get; set; }

    public static void AddPoint(ChartControl chartControl, object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var chart = chartControl.FindControl<CartesianChart>("Chart");
        // Get point position.
        var p = e.GetPosition(chart);
        // Scales the UI.
        // ScaleUIPoint returns double[].
        var scaledPoint = chart.ScaleUIPoint(new LvcPoint((float)p.X, (float)p.Y));
        var x = scaledPoint[0];
        var y = scaledPoint[1];
        if (IsW01Selected)
        {
            W01Points.Add(new ObservablePoint(x, y));
        }
        else
        {
            W12Points.Add(new ObservablePoint(x, y));
        }
    }

    private static bool IsW01Selected { get; set; } = true;

    protected override void OnActivated()
    {
        Messenger.Register<ChartViewModel, ValueChangedMessage<bool>, string>(this, "w01channel",
            (recipient, message) => ChartViewModel.IsW01Selected = message.Value);
    }
}