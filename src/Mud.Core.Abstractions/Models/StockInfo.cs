using System.Text.RegularExpressions;

namespace Mud.Core.Models;

public class StockInfo
{
    public string Symbol { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SymbolType { get; set; } = string.Empty;

    public string FullSymbol => $"{SymbolType}{Symbol}";

    public static StockInfo? Parse(string line)
    {
        var data = line.Split('~', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 3)
        {
            return null;
        }
        return new StockInfo { Symbol = data[1], Name = Regex.Unescape( data[2]), SymbolType = data[0] };
    }

    public override int GetHashCode()
    {
        return FullSymbol.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is StockInfo stockInfo)
        {
            return FullSymbol.Equals(stockInfo.FullSymbol);
        }
        return false;
    }
}
