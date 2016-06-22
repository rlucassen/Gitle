namespace Gitle.Web.Controllers
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using Model.Enum;
    using Model.Nested;
    using NHibernate;
    using NHibernate.Linq;

    #endregion

    public class DatabaseController : BaseController
    {
        public DatabaseController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        public void DemoData(long id)
        {
            Redirect("home", "index");
        }

        public void Index()
        {
            var user = new User
                                {
                                    EmailAddress = "robin@lucassen.me",
                                    Name = "user",
                                    Password = new Password("toeter")
                                };
            session.Save(user);
            
            //var adminUser = new User
            //                    {
            //                        EmailAddress = "robin@lucassen.me",
            //                        Name = "admin",
            //                        Password = new Password("toeter")
            //                    };
            //session.Save(adminUser);

            Redirect("home", "index");
        }

        public void MigrateInvoices()
        {
            var invoices = session.Query<Invoice>().ToList();

            var lostBookings = new List<Booking>();
            var linesWithoutBookings = new List<InvoiceLine>();

            using (var tx = session.BeginTransaction())
            {
                foreach (var invoice in invoices)
                {
                    foreach (var invoiceLine in invoice.Lines)
                    {
                        if (invoiceLine.Issue != null)
                        {
                            invoiceLine.Bookings = invoice.Bookings.Where(x => x.Issue == invoiceLine.Issue).ToList();
                        }
                        else
                        {
                            var bookings = invoice.Bookings.Where(x => x.Comment == invoiceLine.Description).ToList();
                            if (bookings.Count == 1)
                            {
                                invoiceLine.Bookings.Add(bookings.First());
                            }
                        }
                    }

                    lostBookings.AddRange(invoice.Bookings.Where(b => !invoice.Lines.SelectMany(l => l.Bookings).Contains(b)));
                    linesWithoutBookings.AddRange(invoice.Lines.Where(x => x.Bookings.Count == 0));

                    session.SaveOrUpdate(invoice);
                }
                tx.Commit();
            }

            PropertyBag.Add("lostBookings", lostBookings);
            PropertyBag.Add("linesWithoutBookings", linesWithoutBookings);

        }
    }
}