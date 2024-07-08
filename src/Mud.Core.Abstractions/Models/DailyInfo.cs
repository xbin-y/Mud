namespace Mud.Core.Models;

public class DailyInfo
{
    public DateTime Date { get; set; }
    public double High { get; set; }
    public double Open { get; set; }
    public double Close { get; set; }
    public double Low { get; set; }

    public double Volume { get; set; }

    public double PreClose { get; set; }

    public double Change { get; set; }
}
