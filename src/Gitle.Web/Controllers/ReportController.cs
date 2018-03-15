namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Helpers;
    using Model.Helpers;
    using QueryParsers;
    using Gitle.Model;
    using NHibernate;
    using System.Linq;
    using Gitle.Model.James;
    using NHibernate.Linq;

    public class ReportController : SecureController
    {
        public ReportController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index(string query)
        {
            var parser = new BookingQueryParser(session, query, CurrentUser);

            var reportPresets = session.Query<ReportPreset>().Where(x => x.User == CurrentUser && x.IsActive);
            var globalReportPresets = session.Query<ReportPreset>().Where(x => x.User == null && x.IsActive);

            PropertyBag.Add("result", parser);

            PropertyBag.Add("reportPresets", reportPresets);
            PropertyBag.Add("globalReportPresets", globalReportPresets);

            PropertyBag.Add("allUsers", session.Query<User>().Where(x => x.CanBookHours && x.IsActive).OrderBy(x => x.FullName).ToList());

            var allCustomers = session.Query<Customer>().Where(x => x.IsActive);
            var allApplications = session.Query<Application>().Where(x => x.IsActive);
            var allProjects = session.Query<Project>().Where(x => x.IsActive);
            if (parser.Applications.Count > 0)
            {
                allProjects = allProjects.Where(x => parser.Applications.Contains(x.Application));
            }
            if (parser.Customers.Count > 0)
            {
                allApplications = allApplications.Where(x => parser.Customers.Contains(x.Customer));
                allProjects = allProjects.Where(x => parser.Customers.Contains(x.Application.Customer));
            }
            PropertyBag.Add("allCustomers", allCustomers.OrderBy(x => x.Name).ToList());
            PropertyBag.Add("allApplications", allApplications.OrderBy(x => x.Name).ToList());
            PropertyBag.Add("allProjects", allProjects.OrderBy(x => x.Name).ToList());
            PropertyBag.Add("selectedProjects", parser.Projects);

            var presetDates = new List<dynamic>();
            var today = DateTime.Today;

            presetDates.Add(new DatePreset { startDate = today.StartOfWeek(), endDate = today.EndOfWeek(), title = "Deze week"});

            for (int i = 1; i <= 8; i++)
            {
                today = today.AddDays(-7);
                presetDates.Add(new DatePreset{ startDate = today.StartOfWeek(), endDate = today.EndOfWeek(), title = $"Week {today.WeekNr()}"});
            }

            PropertyBag.Add("presetDates", presetDates);
        }

        [Admin]
        public void ExportCsv(string query)
        {
            var parser = new BookingQueryParser(session, $"{query} take:all", CurrentUser);
            var bookings = parser.AllBookings();

            var csv = CsvHelper.BookingsCsv(bookings);
            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = $"bookings_{parser.StartDate:yyyyMMdd_hhmm}_{parser.EndDate:yyyyMMdd_hhmm}.csv";

            Response.AppendHeader("content-disposition", $"attachment; filename={filename}");
            Response.ContentType = "application/csv";

            var byteArray = Encoding.Default.GetBytes(csv);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }

        public void ExportWeeks()
        {
            var employees = session.Query<User>().Where(x => x.JamesEmployeeId > 0).ToList();
            var exportweeks = new List<ExportWeeksGitleVsJames>();
            var sqlConnectionHelper = new SqlConnectionHelper();

            foreach (var employee in employees)
            {
                var bookings = session.Query<Booking>().Where(x => x.IsActive && x.Date.Year == DateTime.Today.Year && x.User.Id == employee.Id).ToList();
                var exportWeek = new ExportWeeksGitleVsJames {NameOfEmployee = employee.FullName};

                for (int i = 0; i < 53; i++)
                {
                    exportWeek.Weeks[i].MinutesGitle = bookings.Where(x => x.Date.WeekNr() == i+1).Sum(x => x.Minutes);

                    using (var reader = sqlConnectionHelper.ExecuteSqlQuery("james", "SELECT SUM(datediff(mi, wd.StartTijd, wd.EindTijd)) - ISNULL((SELECT SUM(r.DuurMinuten) " +
                                                                                                                                                    "FROM [Registratie] r " +
                                                                                                                                                    "JOIN Werkdag wd on wd.Id = r.Werkdag " +
                                                                                                                                                    "JOIN [Week] w on w.Id = wd.[Week] " +
                                                                                                                                                    "WHERE r.RegistratieType IN (0,1,3,6,11) " +
                                                                                                                                                    "AND w.Medewerker = " + employee.JamesEmployeeId +
                                                                                                                                                    "AND w.WeekNr = " + i+1 +
                                                                                                                                                    "AND w.Jaar = " + DateTime.Today.Year + "),0) " +
                                                                                    "FROM Werkdag wd " +
                                                                                    "JOIN [Week] w on w.Id = wd.[Week] " +
                                                                                    "JOIN [Medewerker] m on m.Id = w.Medewerker " +
                                                                                    "WHERE w.Medewerker = " + employee.JamesEmployeeId +
                                                                                    "AND w.Jaar = " + DateTime.Today.Year +
                                                                                    "AND w.WeekNr = " + i+1 +
                                                                                    "GROUP BY w.WeekNr, w.Jaar, w.Medewerker"))
                    {
                        while (reader.Read())
                        {
                            exportWeek.Weeks[i].MinutesJames = (double)reader[0];
                        }
                    }

                    sqlConnectionHelper.CloseSqlConnection();
                }

                exportweeks.Add(exportWeek);
            }

            var csv = CsvHelper.ExportWeeks(exportweeks);
            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = $"gitle_vs_james_{DateTime.Today:yyyyMMdd_hhmm}.csv";

            Response.AppendHeader("content-disposition", $"attachment; filename={filename}");
            Response.ContentType = "application/csv";

            var byteArray = Encoding.Default.GetBytes(csv);
            var stream = new MemoryStream(byteArray);
            Response.BinaryWrite(stream);
        }
    }

    public class DatePreset
    {
        public string title;
        public DateTime endDate;
        public DateTime startDate;
    }
}