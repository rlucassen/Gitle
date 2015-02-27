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
        private readonly ISession session;

        public BookingController(ISessionFactory sessionFactory)
        {
            this.session = sessionFactory.GetCurrentSession();
        }

        public void Index()
        {
            PropertyBag.Add("projects", session.Query<Project>().ToList());
        }

        public void Save()
        {
            var booking = BindObject<Booking>("booking");
            booking.User = CurrentUser;

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(booking);
                transaction.Commit();
            }
            RedirectToReferrer();
        }
    }
}