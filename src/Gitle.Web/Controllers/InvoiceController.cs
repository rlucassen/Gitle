using Gitle.Model;
using Gitle.Model.Enum;
using Gitle.Model.Interfaces.Service;
using Gitle.Web.Helpers;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Gitle.Web.Controllers
{
    using Model.Helpers;

    public class InvoiceController : SecureController
    {
        private readonly IPdfExportService pdfExportService;

        public InvoiceController(ISessionFactory sessionFactory, IPdfExportService pdfExportService) : base(sessionFactory)
        {
            this.pdfExportService = pdfExportService;
        }

        [Admin]
        public void Index()
        {
            var invoices = session.Query<Invoice>().OrderBy(x => x.CreatedAt).ToList().OrderBy(x => (int)(x.State));
            PropertyBag.Add("invoices", invoices);
        }

        [Admin]
        public void Create(long projectId, DateTime startDate, DateTime endDate, bool oldBookings)
        {
            var project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Id == projectId);

            var bookings = session.Query<Booking>().Where(x => x.IsActive && x.Project == project);

            if(oldBookings){
                bookings = bookings.Where(x => x.Date <= endDate && (x.Date >= startDate || x.Invoices.Count(i => i.State == InvoiceState.Definitive) == 0));
            } else {
                bookings = bookings.Where(x => x.Date <= endDate && x.Date >= startDate);
            }

            var invoice = new Invoice(project, startDate, endDate, bookings.ToList());
            
            PropertyBag.Add("invoice", invoice);
            PropertyBag.Add("project", project);
        }

        [Admin]
        public void Copy(string projectSlug, long invoiceId)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);

            PropertyBag.Add("invoice", invoice);
            PropertyBag.Add("project", project);

            RenderView("create");
        }

        [Admin]
        public void Edit(string projectSlug, long invoiceId)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);

            PropertyBag.Add("invoice", invoice);
            PropertyBag.Add("project", project);

            RenderView("create");
        }

        [Admin]
        public void Save(string projectSlug, long invoiceId)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);
            if (invoiceId > 0)
            {
                BindObjectInstance(invoice, "invoice");
            }
            else
            {
                invoice = BindObject<Invoice>("invoice");
            }


            var lines = BindObject<InvoiceLine[]>("lines");
            var bindObject = BindObject<Correction[]>("corrections");
            var corrections = bindObject.Where(x => x.Price != 0.0).ToArray();
            var bookingIds = BindObject<long[]>("bookings");
            var bookings = session.Query<Booking>().Where(x => bookingIds.Contains(x.Id)).ToArray();

            invoice.Bookings = bookings;
            invoice.CreatedBy = CurrentUser;
            invoice.CreatedAt = DateTime.Now;
            invoice.State = InvoiceState.Concept;

            foreach (var line in invoice.Lines.ToList())
            {
                invoice.RemoveLine(line);
            }
            foreach (var line in lines)
            {
                invoice.AddLine(line);
            }

            foreach (var correction in invoice.Corrections.ToList())
            {
                invoice.RemoveCorrection(correction);
            }
            foreach (var correction in corrections)
            {
                invoice.AddCorrection(correction);
            }

            using (var tx = session.BeginTransaction())
            {

                session.SaveOrUpdate(invoice);
                tx.Commit();
            }

            RedirectToAction("index");
        }

        [Admin]
        public void Definitive(string projectSlug, long invoiceId)
        {
            ChangeState(projectSlug, invoiceId, InvoiceState.Definitive);
        }

        [Admin]
        public void Archive(string projectSlug, long invoiceId)
        {
            ChangeState(projectSlug, invoiceId, InvoiceState.Archived);
        }

        [Admin]
        public void ArchiveIssues(string projectSlug, long invoiceId)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);

            using (var tx = session.BeginTransaction())
            {
                foreach (var issue in invoice.Issues)
                {
                    issue.Archive(CurrentUser);
                    session.SaveOrUpdate(issue);
                }
                tx.Commit();
            }

            RedirectToReferrer();
        }

        [Admin]
        public void Download(string projectSlug, long invoiceId)
        {
            CancelView();

            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);

            var pdf = pdfExportService.ConvertHtmlToPdf("invoice", new Dictionary<string, object>{
                {"invoice", invoice},

            });

            var filename = string.Format("{0} - {1} - {2:yyyyMMddHHmm}.pdf", invoice.Number, invoice.Title, DateTime.Now);

            var pdfMemory = new MemoryStream(pdf);

            Context.Response.ContentType = "application/pdf";

            var buffer = new byte[pdfMemory.Length];
            pdfMemory.Position = 0;
            pdfMemory.Read(buffer, 0, pdf.Length);

            Context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            Response.BinaryWrite(pdf);
            Response.AppendHeader("content-disposition", string.Format("filename={0}", filename));
        }

        private void ChangeState(string projectSlug, long invoiceId, InvoiceState state)
        {
            var project = session.SlugOrDefault<Project>(projectSlug);
            var invoice = session.Query<Invoice>().FirstOrDefault(i => i.Id == invoiceId && i.Project == project);

            invoice.State = state;

            using (var tx = session.BeginTransaction())
            {
                session.SaveOrUpdate(invoice);
                tx.Commit();
            }

            RedirectToReferrer();
        }
    }
}