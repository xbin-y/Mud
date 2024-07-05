using LiveChartsCore.Defaults;

namespace Mud.Windows.Models;

public class StockFinancialPointI : FinancialPointI
{
    public double Volume { get; set; }

    public double Change { get; set; }

    public double PreClose { get; set; }

    public StockFinancialPointI(double high, double open, double close, double low) : base(high, open,
        close, low)
    {
    }
}
