namespace Gitle.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CsvHelper
    {
        private static readonly string lineEnd = Environment.NewLine;
        private static readonly string fieldseparator = ";";

        public static string IssuesCsv(Project project, IList<Issue> issues)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}{1}";

            string header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Id",
                                          "Naam",
                                          "Developers",
                                          "Uren",
                                          "Totale schatting",
                                          "Prijs",
                                          "Beschrijving",
                                          "Voltooid",
                                          "Labels");

            string rows = string.Empty;
            foreach (Issue issue in issues)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      issue.Number,
                                      issue.Name,
                                      issue.Devvers,
                                      issue.Hours,
                                      issue.TotalHours,
                                      issue.TotalHours*project.HourPrice,
                                      string.IsNullOrEmpty(issue.Body)
                                          ? string.Empty
                                          : issue.Body.Replace(lineEnd, "").TrimStart('-'),
                                      issue.State,
                                      string.Join(", ", issue.Labels.Select(l => l.Name))
                    );
            }

            return string.Format("{0}{1}", header, rows);
        }
    }
}