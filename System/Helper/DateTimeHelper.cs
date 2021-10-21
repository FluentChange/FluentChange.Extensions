using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class DateTimeHelper
    {
        public static int TotalMonthsUntil(this DateTime from, DateTime to)
        {
            //if (to > from)
            //{
            //var jahre = Math.Max(0, to.Year - from.Year);
            var jahre = to.Year - from.Year;
            var monate = to.Month - from.Month; // kann z.b +5 oder -5 sein

            var monateGesamt = jahre * 12 + monate;
            return monateGesamt;
            //}
            //else
            //{
            //    var jahre = Math.Max(0, to.Year - from.Year);
            //    var monate = to.Month - from.Month; // kann z.b +5 oder -5 sein

            //    var monateGesamt = -(-jahre * 12) + -monate;
            //    return monateGesamt;
            //}


        }

        public static int CountMonthsBetween(this DateTime from, DateTime to)
        {
            if (from > to) throw new ArgumentException();

            var startYear = from.Year;
            var startMonth = from.Month;

            var endYear = to.Year;
            var endMonth = to.Month;

            var resultMonths = (endMonth - startMonth) + 1; // add 1 because we also want to count the first month

            if (startYear != endYear)
            {
                var years = (endYear - startYear);
                resultMonths += years * 12;
            }

            return resultMonths;
        }

        public static DateTime AddYearsDecimal(this DateTime datum, double yearsDecimal)
        {
            var yearsFull = (int)Math.Floor(yearsDecimal);
            var restMonthsDecimal = yearsDecimal - yearsFull;
            var restMonth = (int)Math.Floor(restMonthsDecimal * 12);

            return datum.AddYears(yearsFull).AddMonths(restMonth);
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        public static IEnumerable<DateTime> FilterNull(this IEnumerable<DateTime?> dates)
        {
            return dates.Where(d => d.HasValue).Select(d => d.Value);
        }

        public static DateTime? MinOrNull(this IEnumerable<DateTime> dates)
        {
            if (dates.Count() > 0)
            {
                return dates.Min();
            }
            else
            {
                return null;
            }
        }


        // TODO: move this to extensions
        public static DateTime? MaxOrNull(this IEnumerable<DateTime> dates)
        {
            if (dates.Count() > 0)
            {
                return dates.Max();
            }
            else
            {
                return null;
            }
        }

        public static int Quarter(this DateTime date)
        {
            return GetQuarter(date.Month);
        }

        public static int GetQuarter(int month)
        {
            if (month < 1 || month > 12) throw new ArgumentOutOfRangeException();
            if (month >= 4 && month <= 6)
                return 1;
            else if (month >= 7 && month <= 9)
                return 2;
            else if (month >= 10 && month <= 12)
                return 3;
            else
                return 4;
        }

        public static int FirstMonthInQuarter(int quarter)
        {
            if (quarter < 1 || quarter > 4) throw new ArgumentOutOfRangeException();
            if (quarter == 1) return 1;
            else if (quarter == 2) return 4;
            else if (quarter == 3) return 7;
            else return 10;
        }

        public static int LastMonthInQuarter(int quarter)
        {
            if (quarter < 1 || quarter > 4) throw new ArgumentOutOfRangeException();
            if (quarter == 1) return 3;
            else if (quarter == 2) return 6;
            else if (quarter == 3) return 9;
            else return 12;
        }
    }
}
