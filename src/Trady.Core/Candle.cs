using System;
using Trady.Core.Infrastructure;

namespace Trady.Core
{
    public class Candle : IOhlcv
    {
        public Candle(DateTimeOffset dateTime, decimal open, decimal high, decimal low, decimal close, decimal volume)
        {
            DateTime = dateTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public DateTimeOffset DateTime { get; }

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }

        public override int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return DateTime.Equals(obj);
        }
    }
}
