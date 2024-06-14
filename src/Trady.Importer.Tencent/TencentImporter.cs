using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trady.Core;
using Trady.Core.Infrastructure;
using Trady.Core.Period;

namespace Trady.Importer.Tencent;

public class TencentImporter : IImporter
{
    /// <summary>
    /// Imports the async. Endtime stock history exclusive
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="symbol">Symbol.</param>
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    /// <param name="period">Period.</param>
    /// <param name="token">Token.</param>
    public async Task<IReadOnlyList<IOhlcv>> ImportAsync(string symbol, DateTime? startTime = default(DateTime?),
        DateTime? endTime = default(DateTime?), PeriodOption period = PeriodOption.Daily,
        CancellationToken token = default(CancellationToken))
    {
        if (period != PeriodOption.Daily && period != PeriodOption.Weekly && period != PeriodOption.Monthly)
            throw new ArgumentException("This importer only supports daily, weekly & monthly data");
        var periodValue = period switch
        {
            PeriodOption.Daily => "day",
            PeriodOption.Weekly => "week",
            PeriodOption.Monthly => "month",
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
        using var httpClient = new HttpClient();
        var lowerSymbol = symbol.ToLower();
        var url =
            $"https://ifzq.gtimg.cn/appstock/app/kline/kline?param={lowerSymbol},{periodValue},{startTime?.Date.ToString("yyyy-MM-dd") ?? string.Empty},{endTime?.Date.ToString("yyyy-MM-dd") ?? string.Empty},2000";
        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), token);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var obj = JsonConvert.DeserializeObject<JObject>(json);
        var tokenObj = obj.SelectToken($"$.data.{lowerSymbol}.{periodValue}");
        var arr = tokenObj!.ToObject<List<List<object>>>();
        return arr.Select(t => new Candle(DateTimeOffset.Parse(t[0].ToString()), decimal.Parse(t[1].ToString()), decimal.Parse(t[3].ToString()),
            decimal.Parse(t[4].ToString()), decimal.Parse(t[2].ToString()), decimal.Parse(t[5].ToString()))).ToArray();
    }
}
