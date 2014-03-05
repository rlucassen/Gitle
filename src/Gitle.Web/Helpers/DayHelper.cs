namespace Gitle.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public static class DayHelper
    {
        private const string dateFormat = "yyyy-MM-dd";

         public static Dictionary<string, string> GetPastDaysList()
         {
             var date = DateTime.Today;
             var days = new Dictionary<string, string>();

             days.Add("Vandaag", date.ToString(dateFormat));
             date = date.AddDays(-1);
             days.Add(string.Format("Gisteren ({0:dd MMM})", date), date.ToString(dateFormat));
             date = date.AddDays(-1);
             while (date > DateTime.Today.AddDays(-7))
             {
                 days.Add(string.Format("{0} ({1:dd MMM})", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(date.ToString("dddd")), date), date.ToString(dateFormat));
                 date = date.AddDays(-1);
             }
             return days;
         }
    }
}