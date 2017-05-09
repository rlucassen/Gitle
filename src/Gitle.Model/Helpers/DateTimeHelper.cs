namespace Gitle.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NHibernate.Linq.Functions;

    public static class DateTimeHelper
    {
        public static string Relative(this DateTime? dateTime)
        {
            var ts = dateTime.Value.Kind == DateTimeKind.Utc
                ? new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Value.Ticks)
                : new TimeSpan(DateTime.Now.Ticks - dateTime.Value.Ticks);

            var future = ts.Ticks < 0;
            var delta = Math.Abs(ts.TotalSeconds);

            // These values are approximate and used to calculate a friendly
            // approximation of a date time in the past or future.
            const int second = 1;
            const int minute = 60 * second;
            const int hour = 60 * minute;
            const int day = 24 * hour;
            const int month = 30 * day;
            
            if (delta <= 5 * second)
            {
                return "zojuist";
            }
            if (delta < 1 * minute)
            {
                var message = future ? "over {0} seconden" : "{0} seconden geleden";
                return string.Format(message, Math.Abs(ts.Seconds));
            }
            if (delta < 2 * minute)
            {
                return future ? "over een minuut" : "een minuut geleden geleden";
            }
            if (delta < 45 * minute)
            {
                var message = future ? "over {0} minuten" : "{0} minuten geleden";
                return string.Format(message, Math.Abs(ts.Minutes));
            }
            if (delta < 90 * minute)
            {
                return future ? "over een uur" : "een uur geleden";
            }
            if (delta < 24 * hour)
            {
                var message = future ? "over {0} uur" : "{0} uur geleden";
                return string.Format(message, Math.Abs(ts.Hours));
            }
            if (delta < 36 * hour)
            {
                return future ? "over een dag" : "een dag geleden";
            }
            if (delta < 30 * day)
            {
                var message = future ? "over {0} dagen" : "{0} dagen geleden";
                return string.Format(message, Math.Abs(ts.Days));
            }
            if (delta < 12 * month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                if (months <= 1)
                {
                    return future ? "over een maand" : "een maand geleden";
                }
                var message = future ? "over {0} maanden" : "{0} maanden geleden";
                return string.Format(message, months);
            }
            else
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                if (years <= 1)
                {
                    return future ? "over 1 jaar" : "1 jaar geleden";
                }
                var message = future ? "over {0} jaar" : "{0} jaren geleden";
                return string.Format(message, years);
            }
        }

        public static string Readable(this DateTime? dateTime)
        {
            return dateTime.Value != null ? Readable(dateTime.Value) : "";
        }

        public static string Readable(this DateTime dateTime)
        {
            return dateTime.ToString("MMM d, yyyy HH:mm").ToCamelCase();
        }

        public static string MinutesToTime(int minutes)
        {
            return string.Format("{0}:{1:00}", Math.Floor(minutes/60.0), minutes - (Math.Floor(minutes/60.0) * 60));
        }

        public static DateTime StartOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }

        public static DateTime StartOfWeek(this DateTime dateTime)
        {
            var diff = dateTime.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
            {
                diff += 7;
            }

            return dateTime.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dateTime)
        {
            var diff = DayOfWeek.Sunday - dateTime.DayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dateTime.AddDays(diff).Date;
        }

        public static int WeekNr(this DateTime dateTime)
        {
            if (DateTimeFormatInfo.CurrentInfo != null) return DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return 0;
        }

        public static List<(int year, int week)> WeekNrs(DateTime start, DateTime end)
        {
            var startOfWeek = start.StartOfWeek();

            var weeks = new List<(int year, int week)>();

            var currentStartOfWeek = startOfWeek;
            while (currentStartOfWeek <= end.StartOfWeek())
            {
                weeks.Add((currentStartOfWeek.Year, currentStartOfWeek.WeekNr()));
                currentStartOfWeek = currentStartOfWeek.AddDays(7);
            }

            return weeks;
        }
    }
}