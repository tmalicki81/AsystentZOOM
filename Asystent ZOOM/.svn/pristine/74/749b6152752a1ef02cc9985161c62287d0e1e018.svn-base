using System;
using System.Collections.Generic;
using System.Text;

namespace AsystentZOOM.Plugins.JW.Common
{
    public static class WeekHelper
    {
        public static List<(DateTime monday, DateTime sunday)> GetWeeksInMonth(DateTime date)
        {
            var result = new List<(DateTime monday, DateTime sunday)>();
            var firstDayInMonth = new DateTime(date.Year, date.Month, 1);

            for (int day = 1; day <= DateTime.DaysInMonth(date.Year, date.Month); day++)
            {
                DateTime g = firstDayInMonth.AddDays(day - 1);
                if (g.Month != date.Month) break;
                if (g.DayOfWeek == DayOfWeek.Monday)
                {
                    result.Add((g, g.AddDays(6)));
                }
            }
            return result;
        }

        public static DateTime GetDayOfWeek(DateTime dateInWeek, DayOfWeek dayOfWeek)
        {
            DateTime monday = dateInWeek;
            while (monday.DayOfWeek != DayOfWeek.Monday)
            {
                monday = monday.AddDays(-1).Date;
            }
            DateTime dateTime = monday;
            while (dateTime.DayOfWeek != dayOfWeek)
            {
                dateTime = dateTime.AddDays(1).Date;
            }
            return dateTime;
        }
    }
}
