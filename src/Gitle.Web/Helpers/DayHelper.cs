namespace Gitle.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Model;

    public static class DayHelper
    {
        private const string dateFormat = "yyyy-MM-dd";

         public static Dictionary<string, string> GetPastDaysList(Setting setting)
         {
             var date = DateTime.Today;
             var days = new Dictionary<string, string>();

             AddDateToDays("Vandaag", date);
             date = date.AddDays(-1);
             AddDateToDays(string.Format("Gisteren ({0:dd MMM})", date), date);
             date = date.AddDays(-1);
             while (date > DateTime.Today.AddDays(-7))
             {
                 AddDateToDays(string.Format("{0} ({1:dd MMM})", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(date.ToString("dddd")), date), date);
                 date = date.AddDays(-1);
             }
             return days;

             void AddDateToDays(string dayName, DateTime dateTime)
             {
                 if (date > setting.ClosedForBookingsBefore.GetValueOrDefault()) days.Add(dayName, dateTime.ToString(dateFormat));
             }
         }
    }
}