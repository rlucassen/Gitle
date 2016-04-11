using Gitle.Model;
using NHibernate;
using System.Linq;
using NHibernate.Linq;

namespace Gitle.Web.Controllers
{
    using System.Collections.Generic;
    using Helpers;
    using QueryParsers;

    public class ReportController : SecureController
    {
        public ReportController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [Admin]
        public void Index(string query)
        {
            var parser = new BookingQueryParser(session, query, 0, 100);

            var reportPresets = session.Query<ReportPreset>().Where(x => x.User == CurrentUser);
            var globalReportPresets = session.Query<ReportPreset>().Where(x => x.User == null);

            PropertyBag.Add("allGroupbys", parser.AllGroupbys);
            PropertyBag.Add("groupedBy", parser.GroupedBy);
            PropertyBag.Add("query", query);
            PropertyBag.Add("groupedBookings", parser.GroupedBookings);

            PropertyBag.Add("startDate", parser.StartDate);
            PropertyBag.Add("endDate", parser.EndDate);

            PropertyBag.Add("reportPresets", reportPresets);
            PropertyBag.Add("globalReportPresets", globalReportPresets);

            PropertyBag.Add("selectedUsers", parser.Users);
            PropertyBag.Add("selectedProjects", parser.Projects);
            PropertyBag.Add("selectedApplications", parser.Applications);
            PropertyBag.Add("selectedCustomers", parser.Customers);

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
        }
    }

}