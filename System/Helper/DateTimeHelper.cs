using System;
using System.Collections.Generic;
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


    }
}
