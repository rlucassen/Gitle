using Gitle.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Linq;

namespace Gitle.Web.Controllers
{
    public class BookingController : SecureController
    {
        public BookingController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void Index()
        {
            var bookings = session.Query<Booking>()
                .Where(x => x.IsActive && x.User == CurrentUser && x.Date > DateTime.Today.AddDays(-14))
                .OrderByDescending(x => x.Date)
                .GroupBy(x => x.Date.Date)
                .ToDictionary(g => new { date = g.Key, total = g.ToList().Sum(x => x.Minutes) }, g => g.ToList());
            PropertyBag.Add("bookings", bookings);
        }

        public void Save()
        {
            var booking = BindObject<Booking>("booking");

            booking.User = CurrentUser;
            if (booking.Issue.Id == 0)
            {
                booking.Issue = null;
            }

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(booking);
                transaction.Commit();
            }
            RedirectToReferrer();
        }

        public void Save(int id, int projectId, int issueId)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id);
            
            if (booking != null)
            {
                BindObjectInstance(booking, "booking");
            }
            if (issueId == 0)
            {
                booking.Issue = null;
            }
            else
            {
                booking.Issue = session.Query<Issue>().FirstOrDefault(x => x.IsActive && x.Id == issueId);
            }

            booking.Project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Id == projectId);
            

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(booking);
                transaction.Commit();
            }
            RedirectToReferrer();
        }

        public void Delete(int id)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id && !x.Invoices.Any());
            if (booking != null)
            {
                booking.Deactivate();

                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(booking);
                    transaction.Commit();
                }
            }
            RedirectToReferrer();
        }

        public void Edit(int id)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id && x.User == CurrentUser); //TODO: admin moet ook kunnen ophalen = andere user
            if (booking == null)
            {
                RedirectToReferrer();
            }
            else
            {
                var suggestion = new Suggestion(booking.Project.Name, booking.Project.Id.ToString());
                PropertyBag.Add("booking", booking);
                PropertyBag.Add("suggestion", suggestion);
                RenderView("_row");
                CancelLayout();
            }
        }

    }
}