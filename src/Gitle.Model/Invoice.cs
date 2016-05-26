using Gitle.Model.Enum;
using System;
using Gitle.Model.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    using Newtonsoft.Json.Bson;

    public class Invoice : ModelBase
    {
        public Invoice()
        {
            VAT = true;
            State = InvoiceState.Concept;
            Lines = new List<InvoiceLine>();
            Bookings = new List<Booking>();
            Corrections = new List<Correction>();
        }

        public Invoice(Project project, DateTime startDate, DateTime endDate)
            : this()
        {
            Project = project;
            HourPrice = project.HourPrice;
            StartDate = startDate;
            EndDate = endDate;
            Title = $"{Project.Name} ({StartDate:dd-MM-yyyy} tot {EndDate:dd-MM-yyyy})";
        }

        public Invoice(Project project, DateTime startDate, DateTime endDate, IList<Booking> bookings)
            : this(project, startDate, endDate)
        {
            Bookings = bookings;
            foreach (var booking in bookings)
            {
                if(booking.Issue != null){
                    var invoiceLine = Lines.FirstOrDefault(il => il.Issue == booking.Issue);
                    if (invoiceLine == null)
                    {
                        invoiceLine = new InvoiceLine()
                        {
                            Description = $"#{booking.Issue.Number} - {booking.Issue.Name}",
                            Issue = booking.Issue,
                            Invoice = this,
                        };
                        Lines.Add(invoiceLine);
                    }
                    invoiceLine.Hours += booking.Hours;
                }
                else
                {
                    Lines.Add(new InvoiceLine() { Description = booking.Comment, Invoice = this, Hours = booking.Hours });
                }
            }
            for (var i = Corrections.Count(); i < 5; i++)
            {
                Corrections.Add(new Correction());
            }
        }

        public virtual string Number { get; set; }
        public virtual string Title { get; set; }
        public virtual decimal HourPrice { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual bool VAT { get; set; }
        public virtual string Remarks { get; set; }
        public virtual InvoiceState State { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual Project Project { get; set; }
        public virtual IList<InvoiceLine> Lines { get; set; }
        public virtual IList<Booking> Bookings { get; set; }
        public virtual IList<Correction> Corrections { get; set; }

        public virtual bool IsConcept { get { return State == InvoiceState.Concept; } }
        public virtual bool IsDefinitive { get { return State == InvoiceState.Definitive; } }
        public virtual bool IsArchived { get { return State == InvoiceState.Archived; } }
        public virtual string StateString { get { return State.GetDescription(); } }

        public virtual decimal TotalExclVat { get { return Lines.Sum(x => x.Price) + Corrections.Sum(x => x.Price); } }
        public virtual decimal TotalVat { get { return TotalExclVat * (VAT ? 0.21M : 0); } }
        public virtual decimal Total { get { return TotalExclVat + TotalVat; } }

        public virtual double TotalHours { get { return Lines.Sum(x => x.Hours); } }

        public virtual int IssueCount { get { return Lines.Count(x => x.Issue != null); } }

        public virtual IList<InvoiceLine> ProjectLines { get { return Lines.Where(x => x.Issue == null).ToList(); } }
        public virtual IList<InvoiceLine> IssueLines { get { return Lines.Where(x => x.Issue != null).ToList(); } }
        public virtual IList<Issue> Issues { get { return Lines.Where(x => x.Issue != null).Select(x => x.Issue).ToList(); } }

        public virtual void AddLine(InvoiceLine line)
        {
            line.Invoice = this;
            Lines.Add(line);
        }

        public virtual void RemoveLine(InvoiceLine line)
        {
            Lines.Remove(line);
        }

        public virtual void AddCorrection(Correction correction)
        {
            correction.Invoice = this;
            Corrections.Add(correction);
        }

        public virtual void RemoveCorrection(Correction correction)
        {
            Corrections.Remove(correction);
        }

    }
}
