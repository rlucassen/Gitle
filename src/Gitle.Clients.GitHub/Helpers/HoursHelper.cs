namespace Gitle.Clients.GitHub.Helpers
{
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TitleHelper
    {
        private static Regex r = new Regex(@"\(.*?\)");

        public static double GetHoursFromTitle(string title)
        {
            var match = TrimmedHoursMatch(title).Split('x').Last();
            double hours;
            var tryParse = double.TryParse(match, out hours);
            return tryParse && !double.IsNaN(hours) ? hours : 0;
        }

        public static int GetDevversFromTitle(string title)
        {
            var parts = TrimmedHoursMatch(title).Split('x');
            var match = parts.Length < 2 ? "1" : parts.First();
            int devvers;
            var tryParse = int.TryParse(match, out devvers);
            return tryParse ? devvers : 0;
        }

        public static string GetNameFromTitle(string title)
        {
            return string.IsNullOrEmpty(HoursMatch(title)) ? title : title.Replace(HoursMatch(title), "").TrimEnd(' ');
        }

        public static string CreateTitle(string title, int devvers, double hours)
        {
            return devvers > 1
                       ? string.Format("{0} ({1}x{2})", GetNameFromTitle(title), devvers, hours)
                       : string.Format("{0} ({1})", GetNameFromTitle(title), hours);
        }

        private static string HoursMatch(string title)
        {
            var match = r.Matches(title).Cast<Match>().Select(p => p.Value).LastOrDefault();
            return string.IsNullOrWhiteSpace(match) ? "" : match;
        }

        private static string TrimmedHoursMatch(string title)
        {
            var trimmed = HoursMatch(title).Trim(new[] { '(', ')' });
            return string.IsNullOrWhiteSpace(trimmed) ? "" : trimmed;
        }
    }

}
