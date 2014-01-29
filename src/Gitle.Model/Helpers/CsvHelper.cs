using System;
using System.Collections.Generic;

namespace Gitle.Model.Helpers
{
    using System.Linq;
    using Clients.GitHub.Models;

    public static class CsvHelper
    {
        static readonly string lineEnd = Environment.NewLine;
        static readonly string fieldseparator = ";";

        public static string IssuesCsv(Project project, List<Issue> issues)
        {
            const string rowTemplate = "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                       "Id",
                                       "Naam",
                                       "Developers",
                                       "Uren",
                                       "Totale schatting",
                                       "Prijs",
                                       "Beschrijving",
                                       "Voltooid",
                                       "Labels");

            var rows = string.Empty;
            foreach (var issue in issues)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      issue.Number,
                                      issue.Name,
                                      issue.Devvers,
                                      issue.Hours,
                                      issue.TotalHours,
                                      issue.Hours*project.HourPrice,
                                      string.IsNullOrEmpty(issue.Body) ? string.Empty : issue.Body.Replace(lineEnd, "").TrimStart('-'),
                                      issue.State,
                                      string.Join(", ", issue.Labels.Select(l => l.Name))
                    );
            }

            return string.Format("{0}{1}", header, rows);
        }
    }
}