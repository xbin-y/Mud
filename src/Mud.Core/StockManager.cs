using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mud.Core.Models;
using Trady.Core.Period;
using Trady.Importer.Tencent;

// ReSharper disable MethodHasAsyncOverload

namespace Mud.Core;

public class StockManager : IStockManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StockManager> _logger;
    private readonly IMemoryCache _memoryCache;

    public StockManager(IHttpClientFactory httpClientFactory, ILogger<StockManager> logger, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<List<StockInfo>> SearchAsync(string keyword)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, $"https://smartbox.gtimg.cn/s3/?v=2&q={keyword}&t=all&c=1");
            var response = httpClient.Send(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            var stockInfos = RegexHelper.GetStockInfos(content);
            return stockInfos;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("搜索股票失败:keyword:{},ex:{ex}", keyword, ex.Message);
            return new List<StockInfo>();
        }
    }

    public async Task<List<StockInfo>> GetFocusOnStockListAsync()
    {
        if (!_memoryCache.Persistent()
            .TryGetValue(AppConst.CacheFocusOnStockListKey, out HashSet<CacheStockInfoModel> list))
        {
            return new List<StockInfo>();
        }
        await Task.CompletedTask;
        return list.OrderByDescending(t=>t.LastUpdateTime).Select(t=>new StockInfo
        {
            Symbol = t.Symbol,
            Name = t.Name,
            SymbolType = t.SymbolType
        }).ToList();
    }

    public async Task AddFocusOnStockAsync(StockInfo stockInfo)
    {
        if (!_memoryCache.Persistent()
                .TryGetValue(AppConst.CacheFocusOnStockListKey, out HashSet<CacheStockInfoModel> list))
        {
            return;
        }
        if (list.Any(tt => tt.FullSymbol == stockInfo.FullSymbol))
        {
            return;
        }
        list.Add(new CacheStockInfoModel
        {
            Symbol = stockInfo.Symbol,
            Name = stockInfo.Name,
            SymbolType = stockInfo.SymbolType,
            LastUpdateTime = DateTime.Now
        });
        await _memoryCache.Persistent().SetAsync(AppConst.CacheFocusOnStockListKey, list);
    }

    public async Task RemoveFocusOnStockAsync(StockInfo stockInfo)
    {
        if (!_memoryCache.Persistent()
                .TryGetValue(AppConst.CacheFocusOnStockListKey, out HashSet<CacheStockInfoModel> list))
        {
            return;
        }
        var item = list.FirstOrDefault(t => t.FullSymbol == stockInfo.FullSymbol);
        if (item != null)
        {
            list.Remove(item);
            await _memoryCache.Persistent().SetAsync(AppConst.CacheFocusOnStockListKey, list);
        }
    }

    public async Task<List<DailyInfo>> GetDailyInfoAsync(string fullSymbol, DateTime start, DateTime? end = null)
    {
        var data = await new TencentImporter().ImportAsync(fullSymbol, start, end ?? DateTime.Today);
        var list = new List<DailyInfo>(data.Count);
        var preIndex = -1;
        foreach (var t in data)
        {
            var info = new DailyInfo
            {
                Date = t.DateTime.DateTime,
                Open = t.Open,
                Close = t.Close,
                High = t.High,
                Low = t.Low,
                Volume = t.Volume
            };
            if (preIndex >= 0)
            {
                info.PreClose = data[preIndex].Close;
                info.Change = (info.Close - info.PreClose) / info.PreClose;
            }
            preIndex++;
            list.Add(info);
        }
        return list;
    }
}
