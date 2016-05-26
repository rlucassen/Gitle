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

            var rows = "";
            foreach (var issue in issues)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      issue.Number,
                                      issue.Name,
                                      issue.Devvers,
                                      issue.Hours,
                                      issue.TotalHours,
                                      issue.TotalHours*project.HourPrice,
                                      string.IsNullOrEmpty(issue.Body)
                                          ? ""
                                          : issue.Body.Replace(lineEnd, "").TrimStart('-'),
                                      issue.IsOpen ? "Nee" : "Ja",
                                      string.Join(", ", issue.Labels.Select(l => l.Name))
                    );
            }

            return $"{header}{rows}";
        }

        public static string BookingsCsv(IList<Booking> bookings)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Minuten",
                                          "Datum",
                                          "Gebruiker",
                                          "Project",
                                          "Taak",
                                          "Billable",
                                          "Opmerking");

            var rows = "";
            foreach (var booking in bookings)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      booking.Minutes,
                                      booking.Date.ToShortDateString(),
                                      booking.User.FullName,
                                      booking.Project.Name,
                                      booking.Issue?.Number,
                                      booking.Unbillable ? "Nee" : "Ja",
                                      booking.Comment
                    );
            }

            return $"{header}{rows}";
        }

        public static string InvoiceCsv(IList<Project> projects)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}\"{11}\"{0}\"{12}\"{0}\"{13}\"{0}\"{14}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Klant",
                                          "Applicatie",
                                          "Project",
                                          "Project Id",
                                          "Type",
                                          "Uurtarief",
                                          "Budget initieel project",
                                          "Facturabele uren bij serviceproject",
                                          "Totaal gemaakte uren",
                                          "Billable gemaakte uren",
                                          "Unbillable gemaakte uren",
                                          "Totaal gefactureerde uren",
                                          "Blijf nog te factureren");

            var rows = "";
            foreach (var project in projects)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      project.Application?.Customer.Name,
                                      project.Application?.Name,
                                      project.Name,
                                      project.Id,
                                      project.TypeString,
                                      project.HourPrice,
                                      project.BudgetHours,
                                      "nog te doen",
                                      project.Bookings.Sum(x => x.Hours),
                                      project.Bookings.Where(x => !x.Unbillable).Sum(x => x.Hours),
                                      project.Bookings.Where(x => x.Unbillable).Sum(x => x.Hours),
                                      "nog te doen",
                                      "nog te doen"
                    );
            }

            return $"{header}{rows}";
        }
    }
}