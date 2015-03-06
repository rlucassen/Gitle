using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    public class InvoiceLine : ModelBase
    {
        public InvoiceLine()
        {
            Hours = 0;
            Null = false;
        }

        public virtual Invoice Invoice { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual string Description { get; set; }
        public virtual double Hours { get; set; }
        public virtual double Price { get { return Null ? 0 : Hours * Invoice.HourPrice; } }
        public virtual bool Null { get; set; }

        public virtual IList<Booking> Bookings { get { return Invoice != null ? Invoice.Bookings.Where(x => x.Issue == Issue).ToList() : new List<Booking>(); } }

        public virtual double EstimateHours { get { return Issue.Hours; } }
        public virtual double BookingHours { get { return Bookings.Sum(x => x.Hours); } }
    }

    public class Correction : ModelBase
    {
        public virtual Invoice Invoice { get; set; }
        public virtual string Description { get; set; }
        public virtual double Price { get; set; }
    }
}
