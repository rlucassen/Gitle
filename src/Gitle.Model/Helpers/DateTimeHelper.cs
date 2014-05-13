namespace Gitle.Model.Helpers
{
    using System;

    public static class DateTimeHelper
    {
        public static string Relative(this DateTime dateTime)
        {
            var ts = dateTime.Kind == DateTimeKind.Utc
                ? new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks)
                : new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);

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

        public static string Readable(this DateTime dateTime)
        {
            return dateTime.ToString("MMM d, yyyy HH:mm").ToCamelCase();
        }
    }
}