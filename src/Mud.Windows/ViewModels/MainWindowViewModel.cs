using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using Mud.Core;
using Mud.Core.Models;
using Mud.Windows.Models;
using ReactiveUI;
using SkiaSharp;

namespace Mud.Windows.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _searchText;

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private ObservableCollection<StockInfo> _focusOnStocks = new();

    [ObservableProperty] private StockInfo? _searchSelectedAlbum;

    [ObservableProperty] private StockInfo? _selectedAlbum;

    [ObservableProperty] private ObservableCollection<StockInfo> _searchResults = new();

    [ObservableProperty] private ObservableCollection<Axis> _xAxes = new();

    [ObservableProperty] private ObservableCollection<ISeries> _series = new();

    [ObservableProperty]
    private IPaint<SkiaSharpDrawingContext> _textPaint =  new SolidColorPaint()
    {
        Color =SKColors.DarkSlateGray,
        SKTypeface = SKFontManager.Default.MatchCharacter('汉')
    };

    [ObservableProperty] private ObservableCollection<ISeries> _volSeries = new ();

    [ObservableProperty] private ObservableCollection<Axis> _volAxis = new();

    public MainWindowViewModel()
    {
        InitializeFocusOnStocks();
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);

        this.WhenValueChanged(t => t.SelectedAlbum)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoUpdateDaily!);
    }

    private async void DoUpdateDaily(StockInfo? stockInfo)
    {
        if (stockInfo == null)
        {
            return;
        }
        var stockManager = Container.ServiceProvider.GetRequiredService<IStockManager>();
        var start = DateTime.Today.AddYears(-1);
        var dailyInfos = await stockManager.GetDailyInfoAsync(stockInfo.FullSymbol, start, DateTime.Today);
        XAxes.Clear();
        Series.Clear();
        VolAxis.Clear();
        var seriesValues = new List<StockFinancialPointI>();
        var xAxeLabels = new List<string>();
        foreach (var dailyInfo in dailyInfos)
        {
            seriesValues.Add(new StockFinancialPointI(dailyInfo.High, dailyInfo.Open,
                dailyInfo.Close, dailyInfo.Low)
            {
                Volume = dailyInfo.Volume,
                Change = dailyInfo.Change,
                PreClose = dailyInfo.PreClose
            });
            xAxeLabels.Add(dailyInfo.Date.ToString("yyyy-MM-dd"));
        }
        XAxes.Add(new Axis
        {
            Labels = xAxeLabels,MinLimit = dailyInfos.Count - 60,MaxLimit = dailyInfos.Count
        });
        VolAxis.Add(new Axis {  Labels = xAxeLabels,MinLimit = dailyInfos.Count - 60,MaxLimit = dailyInfos.Count});
        XAxes[0].SharedWith = VolAxis;
        VolAxis[0].SharedWith = XAxes;

        Series.Add(new CandlesticksSeries<StockFinancialPointI>
        {
            UpStroke = new SolidColorPaint(SKColors.Red),
            UpFill = new SolidColorPaint(SKColors.Red),
            DownStroke = new SolidColorPaint(SKColors.Green),
            DownFill = new SolidColorPaint(SKColors.Green),
            YToolTipLabelFormatter = p =>
            {
                var coordinate = p.Coordinate;
                var interpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 7);
                interpolatedStringHandler.AppendLiteral("最高:");
                interpolatedStringHandler.AppendFormatted(coordinate.PrimaryValue, "C2");
                interpolatedStringHandler.AppendFormatted(Environment.NewLine);
                interpolatedStringHandler.AppendLiteral("开盘:");
                interpolatedStringHandler.AppendFormatted(coordinate.TertiaryValue, "C2");
                interpolatedStringHandler.AppendFormatted(Environment.NewLine);
                interpolatedStringHandler.AppendLiteral("收盘:");
                interpolatedStringHandler.AppendFormatted(coordinate.QuaternaryValue, "C2");
                interpolatedStringHandler.AppendFormatted(Environment.NewLine);
                interpolatedStringHandler.AppendLiteral("最低:");
                interpolatedStringHandler.AppendFormatted(coordinate.QuinaryValue, "C2");
                interpolatedStringHandler.AppendFormatted(Environment.NewLine);
                interpolatedStringHandler.AppendLiteral("成交:");
                interpolatedStringHandler.AppendLiteral(p.Model!.Volume > 10000 ? (p.Model.Volume / 10000).ToString("#0.00") + "万" : p.Model.Volume.ToString("N0"));;
                interpolatedStringHandler.AppendFormatted(Environment.NewLine);
                interpolatedStringHandler.AppendLiteral("涨幅:");
                interpolatedStringHandler.AppendFormatted(p.Model!.Change, "0.00%");
                return interpolatedStringHandler.ToStringAndClear();
            },
            Values = seriesValues.ToArray()
        });
        var ma5 = CreateMaSeries(dailyInfos, 5,SKColors.Coral);
        Series.Add( ma5);
        var ma10 = CreateMaSeries(dailyInfos, 10,SKColors.Yellow);
        Series.Add( ma10);
        var ma20 = CreateMaSeries(dailyInfos, 20,SKColors.Purple);
        Series.Add( ma20);
        VolSeries.Add(new ColumnSeries<DailyInfo>
        {
            Values = dailyInfos,
            Fill = new SolidColorPaint(SKColors.Red),
            Mapping = (d, v) => new Coordinate(d.Volume, v, 0, 0, 0,0,Error.Empty)
        }.OnPointMeasured(point =>
        {
            if (point.Visual is null) return;
            var isDanger = point.Model?.Change >= 0;
            point.Visual.Fill = isDanger
                ? new SolidColorPaint(SKColors.Red)
                : new SolidColorPaint(SKColors.Green); // when null, the serie
        }));
    }

    private LineSeries<double?> CreateMaSeries(List<DailyInfo> daily,int avgDay,SKColor skColor)
    {
        var values = new List<double?>();
        var queue = new Queue<double>();
        for (int i = 0; i < daily.Count; i++)
        {
            if (i < avgDay)
            {
                values.Add(null);
                queue.Enqueue(daily[i].Open);
            }
            else
            {
                if (queue.Count == avgDay)
                {
                    values.Add(queue.Sum() / avgDay);
                }
                queue.Enqueue(daily[i].Open);
                queue.Dequeue();
            }
        }
        return new LineSeries<double?>
        {
            LineSmoothness = 1,
            Fill = null,
            GeometryFill = null,
            GeometryStroke = null,
            Name = $"MA{avgDay}",
            Values = values,
            Stroke = new SolidColorPaint(skColor, 2),
            GeometrySize = 10,
        };
    }

    private async void InitializeFocusOnStocks()
    {
        var stockManager = Container.ServiceProvider.GetRequiredService<IStockManager>();
        var focusOnStocks = await stockManager.GetFocusOnStockListAsync();
        foreach (var album in focusOnStocks)
        {
            FocusOnStocks.Add(album);
        }
    }

    private async void DoSearch(string s)
    {
        IsBusy = true;
        SearchResults.Clear();
        if (!string.IsNullOrWhiteSpace(s))
        {
            var manager = Container.ServiceProvider.GetRequiredService<IStockManager>();
            var albums = await manager.SearchAsync(s);
            foreach (var album in albums)
            {
                SearchResults.Add(album);
            }
        }
        IsBusy = false;
    }
}
