using System;

namespace FluentChange.Extensions.Common.Models
{
    public class MonthPeriod
    {
        public int Year { get;private set; }
        public int Month { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public int DaysInMonth { get; private set; }

        public MonthPeriod()
        {
            
        }
        public MonthPeriod(int year, int month)
        {
            Year = year;
            Month = month;
            Start = new DateTime(Year, Month, 1, 0, 0, 0);
            DaysInMonth = DateTime.DaysInMonth(Year, Month);
            End = new DateTime(Year, Month, DaysInMonth, 23, 59, 59);
        }
    }
}
