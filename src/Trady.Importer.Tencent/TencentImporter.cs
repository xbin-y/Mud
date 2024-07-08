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
        var periodValue = "day";
        switch (period)
        {
            case PeriodOption.Daily:
            {
                periodValue = "day";
                break;
            }
            case PeriodOption.Weekly:
                periodValue = "week";
                break;
            case PeriodOption.Monthly:
                periodValue = "month";
                break;
        }
        using var httpClient = new HttpClient();
        var lowerSymbol = symbol.ToLower();
        var end = endTime ?? DateTime.Today;
        var year = 1;
        if (startTime.HasValue)
        {
            year = (end.Year - startTime.Value.Year) + 1;
        }
        var data = new List<IOhlcv>();
        JObject lastJbo = null;
        if (startTime.HasValue && year > 3)
        {
            var start = startTime.Value;
            var endT = start;
            for (int i = 0; i < year; i=i+2)
            {
                endT = start.AddYears(2);
                var url =
                    $"https://web.ifzq.gtimg.cn/appstock/app/fqkline/get?param={lowerSymbol},{periodValue},{start.Date.ToString("yyyy-MM-dd")},{endT.Date.ToString("yyyy-MM-dd")},{640*3},qfq";
                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), token);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<JObject>(json);
                var tokenObj = obj.SelectToken($"$.data.{lowerSymbol}.qfq{periodValue}");
                lastJbo = obj;
                if (tokenObj != null)
                {
                    var arr = tokenObj!.ToObject<List<List<object>>>();
                    var list = arr.Select(t => new Candle(DateTimeOffset.Parse(t[0].ToString()), decimal.Parse(t[1].ToString()), decimal.Parse(t[3].ToString()),
                        decimal.Parse(t[4].ToString()), decimal.Parse(t[2].ToString()), decimal.Parse(t[5].ToString()))).ToArray();
                    data.AddRange(list);
                }
                start = endT > end ? end : endT;
            }
        }
        else
        {
            var url =
                $"https://web.ifzq.gtimg.cn/appstock/app/fqkline/get?param={lowerSymbol},{periodValue},{startTime?.Date.ToString("yyyy-MM-dd") ?? string.Empty},{endTime?.Date.ToString("yyyy-MM-dd") ?? string.Empty},{640*3},qfq";
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), token);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<JObject>(json);
            lastJbo = obj;
            var tokenObj = obj.SelectToken($"$.data.{lowerSymbol}.qfq{periodValue}");
            var arr = tokenObj!.ToObject<List<List<object>>>();
            var l =arr.Select(t => new Candle(DateTimeOffset.Parse(t[0].ToString()), decimal.Parse(t[1].ToString()), decimal.Parse(t[3].ToString()),
                decimal.Parse(t[4].ToString()), decimal.Parse(t[2].ToString()), decimal.Parse(t[5].ToString()))).ToArray();
            data.AddRange(l);
        }
        var last = lastJbo?.SelectToken($"$.data.{lowerSymbol}.qt.{lowerSymbol}");
        if (last is JArray array)
        {
            var t = array.ToObject<List<object>>();
            var date = DateTimeOffset.ParseExact(t[30].ToString(),"yyyyMMddHHmmss",null).DateTime.Date;
            var candle = new Candle(date,decimal.Parse(t[5].ToString()), decimal.Parse(t[33].ToString()),
                decimal.Parse(t[34].ToString()), decimal.Parse(t[3].ToString()), decimal.Parse(t[6].ToString()));
            data.Add(candle);
        }
        data = data.Distinct().ToList();
        return data;
    }
}
