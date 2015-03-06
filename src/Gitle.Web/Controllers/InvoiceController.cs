using Gitle.Model;
using Gitle.Model.Enum;
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
            var invoices = session.Query<Invoice>();
            PropertyBag.Add("invoices", invoices);
        }

        [Admin]
        public void Create(long projectId, DateTime startDate, DateTime endDate)
        {
            var project = session.Get<Project>(projectId);

            var bookings = session.Query<Booking>().Where(x => x.Date >= startDate && x.Date <= endDate).ToList();

            var invoice = new Invoice(project, startDate, endDate, bookings);
            
            PropertyBag.Add("invoice", invoice);
            PropertyBag.Add("project", project);
        }

        [Admin]
        public void Copy(string projectSlug, string invoiceId)
        {
            var project = session.Query<Project>().FirstOrDefault(p => p.Slug == projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Number == invoiceId && i.Project == project);

            PropertyBag.Add("invoice", invoice);
            PropertyBag.Add("project", project);

            RenderView("create");
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
            invoice.State = InvoiceState.Concept;

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