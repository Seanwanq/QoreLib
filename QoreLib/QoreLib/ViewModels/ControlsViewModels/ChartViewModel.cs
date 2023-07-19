using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using QoreLib.Models;
using QoreLib.Services;
using QoreLib.Views.Controls;
using SkiaSharp;

namespace QoreLib.ViewModels.ControlsViewModels;

public partial class ChartViewModel : ViewModelBase
{
    private static IDatabaseService? _databaseService;
    private static CartesianChart _chart;

    public ChartViewModel(IDatabaseService? databaseService, ChartControl chartControl)
    {
        IsActive = true;
        _databaseService = databaseService;
        _chart = chartControl.FindControl<CartesianChart>("Chart");
        if (_databaseService != null) _databaseService.DatabaseConnectedChanged += OnDatabaseConnectedChanged;
        if (_databaseService != null) IsDatabaseConnected = _databaseService.IsDatabaseConnected;
        Series = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = BaseDataPoints,
                Fill = null,
                GeometrySize = 5,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0,
                DataPadding = new LvcPoint(0, 0),
                Stroke = new SolidColorPaint { Color = SKColors.DodgerBlue, StrokeThickness = 1 }
            },
            new ScatterSeries<ObservablePoint>
            {
                Values = W01Points,
                GeometrySize = 10,
                Fill = new SolidColorPaint(SKColors.LimeGreen),
                DataPadding = new LvcPoint(0, 0)
            },
            new ScatterSeries<ObservablePoint>
            {
                Values = W12Points,
                GeometrySize = 10,
                Fill = new SolidColorPaint(SKColors.BlueViolet),
                DataPadding = new LvcPoint(0, 0)
            }
        };
    }

    private static bool _isDatabaseConnected = false;

    private static bool IsDatabaseConnected
    {
        get => _isDatabaseConnected;
        set
        {
            if(_isDatabaseConnected == value) return;
            _isDatabaseConnected = value;
            OnIsDatabaseConnectedChanged(value);
        }
    }
    
    private static bool _isDataGood = true;

    private static bool IsDataGood
    {
        get => _isDataGood;
        set
        {
            if (_isDataGood == value) return;
            _isDataGood = value;
            OnIsDataGoodChanged(value);
        }
    }

    private static ObservableCollection<ObservablePoint>? BaseDataPoints { get; set; } = new();
    public static ObservableCollection<ObservablePoint> W01Points { get; set; } = new();
    public static ObservableCollection<ObservablePoint> W12Points { get; set; } = new();
    public static ISeries[]? Series { get; set; }
    private static bool IsW01Selected { get; set; } = true;
    
    private static double MinYAxis { get; set; } = 0;

    private static double MaxYAxis { get; set; } = 0;

    private static double MinXAxis { get; set; } = 0;

    private static double MaxXAxis { get; set; } = 0;

    private static int _dataId = 905;
    private static int DataId
    {
        get => _dataId;
        set
        {
            if (_dataId == value) return;
            _dataId =  value;
            OnDataIdChanged(value);
        }
    }

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

    private static void OnIsDatabaseConnectedChanged(bool value)
    {
        if (IsDatabaseConnected)
        {
            try
            {
                BaseDataPoints.AddRange(GetBaseDataPointsById(DataId) ?? throw new Exception("Database connection is not established"));
                IsDataGood = true;
                var xAxis = new Axis { MaxLimit = MaxXAxis, MinLimit = MinXAxis };
                var yAxis = new Axis { MaxLimit = MaxYAxis, MinLimit = MinYAxis };
                _chart.XAxes = new List<ICartesianAxis> { xAxis };
                _chart.YAxes = new List<ICartesianAxis> { yAxis };
            }
            catch (Exception e)
            {
                if (e.Message == "Database connection is not established")
                {
                    _databaseService.CloseScientificDatabaseConnection();
                }
                else
                {
                    IsDataGood = false;
                }

                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                    }), "notificationchannel");
                BaseDataPoints.Clear();
            }
        }
        else
        {
            BaseDataPoints.Clear();
            var xAxis = new Axis { MaxLimit = 10, MinLimit = 0 };
            var yAxis = new Axis { MaxLimit = 10, MinLimit = 0 };
            _chart.XAxes = new List<ICartesianAxis> { xAxis };
            _chart.YAxes = new List<ICartesianAxis> { yAxis };
        }
    }

    // BUG Bad positioning with OnDataIdChanged and OnIsDatabaseConnectedChanged 
    
    private static void OnDataIdChanged(int value)
    {
        if (IsDatabaseConnected)
        {
            try
            {
                BaseDataPoints.Clear();
                BaseDataPoints.AddRange(GetBaseDataPointsById(value) ?? throw new Exception("Database connection is not established"));
                IsDataGood = true;
                var xAxis = new Axis { MaxLimit = MaxXAxis, MinLimit = MinXAxis };
                var yAxis = new Axis { MaxLimit = MaxYAxis, MinLimit = MinYAxis };
                _chart.XAxes = new List<ICartesianAxis> { xAxis };
                _chart.YAxes = new List<ICartesianAxis> { yAxis };
            }
            catch (Exception e)
            {
                if (e.Message == "Database connection is not established")
                {
                    _databaseService.CloseScientificDatabaseConnection();
                }
                else
                {
                    IsDataGood = false;
                }

                WeakReferenceMessenger.Default.Send(
                    new ValueChangedMessage<NotificationModel>(new NotificationModel
                    {
                        Content = e.Message, NoteType = NotificationType.Error, Title = "Error"
                    }), "notificationchannel");
                BaseDataPoints.Clear();
            }
        }
        else
        {
            BaseDataPoints.Clear();
        } 
    }

    private static void OnIsDataGoodChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "datagoodchannel");
    }

    protected override void OnActivated()
    {
        Messenger.Register<ChartViewModel, ValueChangedMessage<bool>, string>(this, "w01channel",
            (recipient, message) => ChartViewModel.IsW01Selected = message.Value);
        Messenger.Register<ChartViewModel, ValueChangedMessage<bool>, string>(this, "databadchannel",
            ((r, m) => ChartViewModel.IsDataGood = !m.Value));
        Messenger.Register<ChartViewModel, ValueChangedMessage<int>, string>(this, "chartidchannel",
            ((r, m) => ChartViewModel.DataId = m.Value));
        Messenger.Register<ChartViewModel, ValueChangedMessage<int>, string>(this, "idchannel",
            ((r, m) => ChartViewModel.DataId = m.Value));
    }

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        IsDatabaseConnected = isConnected;
    }

    private static ObservableCollection<ObservablePoint>? GetBaseDataPointsById(int id)
    {
        var baseDataPoints = _databaseService.SelectDataToObservableCollectionById(id);
        if (baseDataPoints is { Count: > 0 })
        {
            double minXValue = (double)baseDataPoints.Min(p => p.X);
            double maxXValue = (double)baseDataPoints.Max(p => p.X);
            double minYValue = (double)baseDataPoints.Min(p => p.Y);
            double maxYValue = (double)baseDataPoints.Max(p => p.Y);
            double rangeY = Math.Abs(maxYValue - minYValue);
            double rangeX = Math.Abs(maxXValue - minXValue);
            double offsetY = rangeY * 0.1;
            double offsetX = rangeX * 0.1;
            double newMinYAxis = minYValue - offsetY;
            double newMaxYAxis = maxYValue + offsetY;
            double newMinXAxis = minXValue - offsetX;
            double newMaxXAxis = maxXValue + offsetX;
            MaxYAxis = newMaxYAxis;
            MinYAxis = newMinYAxis;
            MaxXAxis = newMaxXAxis;
            MinXAxis = newMinXAxis;
        }

        return baseDataPoints;
    }
}