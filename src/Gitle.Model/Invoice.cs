using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    public class Invoice : ModelBase
    {
        public Invoice()
        {
            VAT = true;
        }

        public virtual string Number { get; set; }
        public virtual string Title { get; set; }
        public virtual int HourPrice { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual bool VAT { get; set; }
        public virtual string Remarks { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual Project Project { get; set; }
        public virtual IList<InvoiceLine> Lines { get; set; }
        public virtual IList<Booking> Bookings { get; set; }
        public virtual IList<Correction> Corrections { get; set; }
    }
}
