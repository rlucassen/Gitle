namespace Gitle.Web.Helpers
{
    using System;

    public static class DayPartHelper
    {
         public static string GetDayPart(DateTime dateTime)
         {
             if (dateTime.Hour >= 6 && dateTime.Hour < 12)
             {
                 return "ochtend";
             }
             if (dateTime.Hour >= 12 && dateTime.Hour < 18)
             {
                 return "middag";
             }
             if (dateTime.Hour > 18)
             {
                 return "avond";
             }
             return "limbo";
         }
    }
}