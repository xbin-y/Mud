namespace Mud.Core.Models;

public class DailyInfo
{
    public DateTime Date { get; set; }
    public decimal High { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Low { get; set; }

    public decimal Volume { get; set; }

    public decimal PreClose { get; set; }

    public decimal Change { get; set; }
}
