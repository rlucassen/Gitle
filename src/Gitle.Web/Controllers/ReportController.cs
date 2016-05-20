using Gitle.Model;
using NHibernate;
using System.Linq;
using NHibernate.Linq;

namespace Gitle.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Helpers;
    using Model.Enum;
    using Model.Helpers;
    using QueryParsers;

    public class ReportController : SecureController
    {
        public ReportController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index(string query)
        {
            var parser = new BookingQueryParser(session, query);

            var reportPresets = session.Query<ReportPreset>().Where(x => x.User == CurrentUser && x.IsActive);
            var globalReportPresets = session.Query<ReportPreset>().Where(x => x.User == null && x.IsActive);

            PropertyBag.Add("result", parser);

            PropertyBag.Add("reportPresets", reportPresets);
            PropertyBag.Add("globalReportPresets", globalReportPresets);

            PropertyBag.Add("allUsers", session.Query<User>().Where(x => x.IsAdmin && x.IsActive).OrderBy(x => x.FullName).ToList());

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

            var presetDates = new List<dynamic>();
            var today = DateTime.Today;

            presetDates.Add(new DatePreset { startDate = today.StartOfWeek(), endDate = today.EndOfWeek(), title = "Deze week"});

            for (int i = 1; i <= 9; i++)
            {
                today = today.AddDays(-7);
                presetDates.Add(new DatePreset{ startDate = today.StartOfWeek(), endDate = today.EndOfWeek(), title = $"Week {today.WeekNr()}"});
            }

            PropertyBag.Add("presetDates", presetDates);
        }

        [Admin]
        public void ExportCsv()
        {
            //var project = session.Slug<Booking>(projectSlug);
            var bookings = session.Query<Booking>().Where(x => x.IsActive).ToList();

            var csv = CsvHelper.ReportCsv(bookings);
            CancelView();

            Response.ClearContent();
            Response.Clear();

            var filename = $"bookings_{DateTime.Now:yyyyMMdd_hhmm}.csv";

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