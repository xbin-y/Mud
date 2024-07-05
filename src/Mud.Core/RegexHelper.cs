using System.Text.RegularExpressions;
using Mud.Core.Models;

namespace Mud.Core;

public partial class RegexHelper
{
    [GeneratedRegex("=\"(?<value>[\\s\\S^\"]+)\"")]
    private static partial Regex StockListResult();

    public static List<StockInfo> GetStockInfos(string input)
    {
        var data = StockListResult().Matches(input).Select(x => x.Groups["value"].Value).SelectMany(t=>t.Split('^',StringSplitOptions.RemoveEmptyEntries)).ToList();
        if (data.Count > 1)
            return data.Select(StockInfo.Parse).Where(t => t != null && t.SymbolType is "sh" or "sz").Select(t => t!)
                .ToList();

        var first = data.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first) || first == "N")
        {
            return [];
        }
        return data.Select(StockInfo.Parse).Where(t=>t != null && t.SymbolType is "sh" or "sz").Select(t=>t!).ToList();
    }
}
