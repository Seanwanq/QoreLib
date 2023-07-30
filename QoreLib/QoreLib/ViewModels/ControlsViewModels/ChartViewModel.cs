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
using Microsoft.CodeAnalysis;
using QoreLib.Models;
using QoreLib.Services;
using QoreLib.ViewModels.PagesViewModels;
using QoreLib.Views.Controls;
using QoreLib.Views.PagesViews;
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
            if (_isDatabaseConnected == value) return;
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
    private static List<double> Omega01Index { get; set; } = new();
    private static List<double> Omega01ValueReal { get; set; } = new();
    private static List<double> HalfOmega02Index { get; set; } = new();
    private static List<double> HalfOmega02ValueReal { get; set; } = new();
    private static SpectrumAdditionalDataModel AdditionalData { get; set; } = new();
    private static bool _isFilled = false;
    private static bool IsFilled
    {
        get => _isFilled;
        set
        {
            if(_isFilled == value) return;
            _isFilled = value;
        }
    }

    private static bool _isUseful = true;

    private static bool IsUseful
    {
        get => _isUseful;
        set
        {
            if (_isUseful == value) return;
            _isUseful = value;
            OnIsUsefulChanged(value);
        }
    }
    private static bool _isOK = false;
    private static bool IsOK
    {
        get => _isOK;
        set
        {
            if (_isOK == value) return;
            _isOK = value;
            OnIsOKChanged(value);
        }
    }
    private static int _dataId = DataPlotPageViewModel.ExposedId;
    private static int DataId
    {
        get => _dataId;
        set
        {
            if (_dataId == value) return;
            _dataId = value;
            OnDataIdChanged(value);
        }
    }

    private static bool _isPlotted = false;

    private static bool IsPlotted
    {
        get => _isPlotted;
        set
        {
            if(_isPlotted == value) return;
            _isPlotted = value;
            OnIsPlottedChanged(value);
        }
    }

    private static void OnIsPlottedChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "isplottedchannel");
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
            Omega01Index.Add(x);
            Omega01ValueReal.Add(y);
            W01Points.Add(new ObservablePoint(x, y));
        }
        else
        {
            HalfOmega02Index.Add(x);
            HalfOmega02ValueReal.Add(y);
            W12Points.Add(new ObservablePoint(x, y));
        }

        IsOK = false;
        IsPlotted = true;
    }

    private static void OnIsDatabaseConnectedChanged(bool value)
    {
        if (IsDatabaseConnected)
        {
            try
            {
                BaseDataPoints.AddRange(GetBaseDataPointsById(DataId) ??
                                        throw new Exception("Database connection is not established"));
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

    private static void OnDataIdChanged(int value)
    {
        if (IsDatabaseConnected)
        {
            try
            {
                BaseDataPoints.Clear();
                ClearPoints();
                BaseDataPoints.AddRange(GetBaseDataPointsById(value) ??
                                        throw new Exception("Database connection is not established"));
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
        if(!value)
            ClearPoints();
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
        Messenger.Register<ChartViewModel, ValueChangedMessage<bool>, string>(this, "isplotokreceivechannel",
            (r, m) => ChartViewModel.IsOK = m.Value);
        Messenger.Register<ChartViewModel, ValueChangedMessage<bool>, string>(this, "isusefulchannel",
            ((r, m) => ChartViewModel.IsUseful = m.Value));
    }

    private void OnDatabaseConnectedChanged(bool isConnected)
    {
        IsDatabaseConnected = isConnected;
    }

    private static ObservableCollection<ObservablePoint>? GetBaseDataPointsById(int id)
    {
        var baseDataPoints = _databaseService?.SelectSpectrumBaseDataToObservableCollectionById(id);
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
            ClearPoints();
            IsFilled = _databaseService.GetSpectrumDataIsFilledById(id);
            if (IsFilled)
            {
                W01Points.AddRange(_databaseService.SelectSpectrumOmega01DataToObservableCollectionById(id));
                W12Points.AddRange(_databaseService.SelectSpectrumHalfOmega02DataToObservableCollectionById(id));
                Omega01Index.AddRange(_databaseService.SelectSpectrumAdditionalToDictionaryListDoubleById(id)["Omega01Index"]);
                Omega01ValueReal.AddRange(_databaseService.SelectSpectrumAdditionalToDictionaryListDoubleById(id)["Omega01ValueReal"]);
                HalfOmega02Index.AddRange(_databaseService.SelectSpectrumAdditionalToDictionaryListDoubleById(id)["HalfOmega02Index"]);
                HalfOmega02ValueReal.AddRange(_databaseService.SelectSpectrumAdditionalToDictionaryListDoubleById(id)["HalfOmega02ValueReal"]);
            }
        }

        return baseDataPoints;
    }

    private static void OnIsOKChanged(bool value)
    {
        if (!value)
        {
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(value), "isplotoksendchannel");
        }
    }

    private static void OnIsUsefulChanged(bool value)
    {
        IsOK = false;
    }

    public static void OKCommand()
    {
        if (_databaseService == null)
        {
            WeakReferenceMessenger.Default.Send(
                new ValueChangedMessage<NotificationModel>(new NotificationModel
                {
                    Content = "Failed to connect to the database.",
                    NoteType = NotificationType.Error,
                    Title = "Error"
                }), "notificationchannel");
        }

        AdditionalData.GroupId = _databaseService!.GetSpectrumDataGroupIdById(DataId);
        AdditionalData.Name = _databaseService.GetSpectrumDataNameById(DataId);
        AdditionalData.ValueType = _databaseService.GetSpectrumDataAppAndTypeById(DataId)["Type"];
        AdditionalData.Omega01Index = Omega01Index;
        AdditionalData.Omega01ValueReal = Omega01ValueReal;
        AdditionalData.HalfOmega02Index = HalfOmega02Index;
        AdditionalData.HalfOmega02ValueReal = HalfOmega02ValueReal;
    }

    public static void SubmitCommand()
    {
        try
        {
            _databaseService.SaveSpectrumAdditionalDataToJsonAndUpdateSqlById(DataId, AdditionalData, IsUseful,
                !IsDataGood);
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(new ValueChangedMessage<NotificationModel>(
                new NotificationModel { Content = e.Message, NoteType = NotificationType.Error, Title = "Error" }));
        }
    }

    public static void ClearPoints()
    {
        W01Points.Clear();
        W12Points.Clear();
        Omega01Index.Clear();
        Omega01ValueReal.Clear();
        HalfOmega02Index.Clear();
        HalfOmega02ValueReal.Clear();
        AdditionalData.Clear();
        IsOK = false;
        IsPlotted = false;
    }
}