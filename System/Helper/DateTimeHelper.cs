using System;
using System.Collections.Generic;
using System.Text;

namespace FluentChange.Extensions.System.Helper
{
    public static class DateTimeHelper
    {
        public static int TotalMonthsUntil(this DateTime rentenEintritt, DateTime date)
        {
         
            var jahre = Math.Max(0, rentenEintritt.Year - date.Year);
            var monate = rentenEintritt.Month - date.Month; // kann z.b +5 oder -5 sein

            var monateGesamt = jahre * 12 + monate;
            return monateGesamt;
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
