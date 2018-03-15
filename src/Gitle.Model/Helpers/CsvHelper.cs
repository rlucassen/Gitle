namespace Gitle.Model.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enum;
    using Gitle.Model.James;
    using NHibernate.Validator.Cfg.Loquacious.Impl;

    public static class CsvHelper
    {
        private static readonly string lineEnd = Environment.NewLine;
        private static readonly string fieldseparator = ";";

        public static string IssuesCsv(Project project, IList<Issue> issues)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}\"{11}\"{0}\"{12}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Taaknummer",
                                          "Naam",
                                          "Geopend op",
                                          "Geopend door",
                                          "Gesloten op",
                                          "Gesloten door",
                                          "Totale schatting in uren",
                                          "Totaal billable boekingen in uren",
                                          "Totaal unbillable boekingen in uren",
                                          "Totaal gefactureerd in uren",
                                          "Labels");

            var rows = "";
            foreach (var issue in issues)
            {
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      issue.Number,
                                      issue.Name,
                                      issue.CreatedAt?.ToString("yyyy-MM-dd"),
                                      issue.CreatedBy?.FullName,
                                      !issue.IsOpen ? issue.ClosedAt?.ToString("yyyy-MM-dd") : "",
                                      !issue.IsOpen ? issue.ClosedBy?.FullName : "",
                                      issue.TotalHours,
                                      issue.BillableBookingHours(),
                                      issue.UnbillableBookingHours(),
                                      issue.TotalHoursInvoiced,
                                      string.Join(", ", issue.Labels.Select(l => l.Name))
                    );
            }

            return $"{header}{rows}";
        }

        public static string BookingsCsv(IList<Booking> bookings)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Minuten",
                                          "Datum",
                                          "Gebruiker",
                                          "Projectnummer",
                                          "Project",
                                          "Taak",
                                          "Billable",
                                          "Opmerking");

            var rows = "";
            foreach (var booking in bookings)
            {
                string issueNumber;
                if (booking.Issue != null) issueNumber = booking.Issue.Number.ToString();
                else issueNumber = "DUMP";

                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      booking.Minutes,
                                      booking.Date.ToShortDateString(),
                                      booking.User.FullName,
                                      booking.Project.Number,
                                      booking.Project.Name,
                                      issueNumber,
                                      booking.Unbillable ? "Nee" : "Ja",
                                      booking.Comment
                    );
            }

            return $"{header}{rows}";
        }

        public static string ExportWeeks(IList<ExportWeeksGitleVsJames> exportWeeksGitleVsJames)
        {
            var numbersOfWeeks = exportWeeksGitleVsJames.Select(x => x.Weeks).Count();
            
            var header = "Medewerker" + fieldseparator;

            for (int i = 0; i < numbersOfWeeks; i++)
                header += "W" + (i + 1) + "_G" + fieldseparator + "W" + (i + 1) + "_J" + fieldseparator;

            header += lineEnd;

            var rows = "";

            foreach (var line in exportWeeksGitleVsJames)
            {
                var row = line.NameOfEmployee + fieldseparator;

                for (int i = 0; i < numbersOfWeeks; i++)
                    row += "" + line.Weeks[i].MinutesGitle + fieldseparator + line.Weeks[i].MinutesJames + fieldseparator;

                rows += row + lineEnd;
            }
            
            return $"{header}{rows}";
        }

        public static string InvoiceCsv(IList<Project> projects)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}\"{11}\"{0}\"{12}\"{0}\"{13}\"{0}\"{14}\"{0}\"{15}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Klant",
                                          "Applicatie",
                                          "Projectnummer",
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
                                      project.Number,
                                      project.Name,
                                      project.Id,
                                      project.TypeString,
                                      project.HourPrice,
                                      project.BudgetHours,
                                      project.SumMaxOfEstimateAndBooking(),
                                      project.TotalHours,
                                      project.BillableHours,
                                      project.UnBillableHours,
                                      project.TotalDefinitiveHours(),
                                      project.ToInvoice()
                    );
            }

            return $"{header}{rows}";
        }

        public static string ProjectCsv(IList<Project> projects)
        {
            const string rowTemplate =
                "\"{2}\"{0}\"{3}\"{0}\"{4}\"{0}\"{5}\"{0}\"{6}\"{0}\"{7}\"{0}\"{8}\"{0}\"{9}\"{0}\"{10}\"{0}\"{11}\"{0}\"{12}\"{0}\"{13}\"{0}\"{14}\"{0}\"{15}\"{0}\"{16}\"{0}{1}";

            var header = string.Format(rowTemplate, fieldseparator, lineEnd,
                                          "Klant",
                                          "Applicatie",
                                          "Projectnummer",
                                          "Project",
                                          "Project Id",
                                          "Type",
                                          "Klantcontacten",
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
                var customers = project.Users.Where(up => !up.User.IsAdmin && !up.User.CanBookHours).ToList();
                var customerString = "";
                var i = 0;
                foreach (var customer in customers)
                {
                    if (i > 0)
                    {
                        customerString = customerString + ", ";
                    }
                    customerString = customerString + customer.User.FullName;
                    i++;
                }
                rows += string.Format(rowTemplate, fieldseparator, lineEnd,
                                      project.Application?.Customer.Name,
                                      project.Application?.Name,
                                      project.Number,
                                      project.Name,
                                      project.Id,
                                      project.TypeString,
                                      customerString,
                                      project.HourPrice,
                                      project.BudgetHours,
                                      project.SumMaxOfEstimateAndBooking(),
                                      project.TotalHours,
                                      project.BillableHours,
                                      project.UnBillableHours,
                                      project.TotalDefinitiveHours(),
                                      project.ToInvoice()
                    );
            }

            return $"{header}{rows}";
        }
    }
}