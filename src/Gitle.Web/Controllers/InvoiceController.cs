using Gitle.Model;
using Gitle.Web.Helpers;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gitle.Web.Controllers
{
    public class InvoiceController : SecureController
    {
        private readonly ISession session;

        public InvoiceController(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        [Admin]
        public void Index()
        {

        }

        [Admin]
        public void Create(long projectId, DateTime startDate, DateTime endDate)
        {
            var project = session.Get<Project>(projectId);

            var bookings = session.Query<Booking>().Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

            var projectBookings = bookings.Where(x => x.Issue == null);
            var issueBookings = new Dictionary<Issue, List<Booking>>();
            foreach (var booking in bookings.Where(x => x.Issue != null))
            {
                if (!issueBookings.ContainsKey(booking.Issue))
                {
                    issueBookings.Add(booking.Issue, new List<Booking>());
                }
                issueBookings[booking.Issue].Add(booking);
            }

            PropertyBag.Add("project", project);
            PropertyBag.Add("projectBookings", projectBookings);
            PropertyBag.Add("issueBookings", issueBookings);
            PropertyBag.Add("startDate", startDate);
            PropertyBag.Add("endDate", endDate);
        }

        [Admin]
        public void Save()
        {
            var invoice = BindObject<Invoice>("invoice");
            var lines = BindObject<InvoiceLine[]>("lines");
            var corrections = BindObject<Correction[]>("corrections").Where(x => x.Price != 0.0).ToArray();
            var bookingIds = BindObject<long[]>("bookings");
            var bookings = session.Query<Booking>().Where(x => bookingIds.Contains(x.Id)).ToArray();

            invoice.Bookings = bookings;
            invoice.CreatedBy = CurrentUser;
            invoice.CreatedAt = DateTime.Now;

            using (var tx = session.BeginTransaction())
            {
                foreach(var line in lines){
                    session.Save(line);
                }
                invoice.Lines = lines;

                foreach (var correction in corrections)
                {
                    session.Save(correction);
                }
                invoice.Corrections = corrections;

                session.SaveOrUpdate(invoice);
                tx.Commit();
            }

            RedirectToAction("index");
        }
    }
}