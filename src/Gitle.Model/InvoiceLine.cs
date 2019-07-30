﻿using System;
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
            Bookings = new List<Booking>();
        }

        public virtual Invoice Invoice { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual string Description { get; set; }
        public virtual double Hours { get; set; }
        public virtual decimal Price => Null ? 0 : (decimal)Hours * Invoice.HourPrice;

        public virtual bool Null { get; set; }

        public virtual IList<Booking> Bookings { get; set; }

        public virtual IList<InvoiceLine> OldInvoiceLines => Issue?.InvoiceLines.Where(x => x.Invoice.IsDefinitive).ToList() ?? new List<InvoiceLine>();

        public virtual double EstimateHours => Issue.TotalHours - OldInvoiceLines.Sum(x => x.Hours);
        public virtual double FullEstimateHours => Issue.TotalHours;
        public virtual double BookingHours => Bookings.Where(x => !x.Unbillable).Sum(x => x.Hours);
        public virtual double BookingHoursUnbillable => Bookings.Where(x => x.Unbillable).Sum(x => x.Hours);
        public virtual double BookingHoursTotal => BookingHours + BookingHoursUnbillable;
        public virtual double Minutes => Hours*60.0;
        public virtual double OldHours => OldInvoiceLines.Where(x => !x.Null).Sum(x => x.Hours);
        public virtual double OldHoursBilled => OldInvoiceLines.Where(x => !x.Null).Sum(x => x.Hours);
        public virtual double OldHoursUnbilled => OldInvoiceLines.Where(x => x.Null).Sum(x => x.Hours);
        public virtual double OldBookingHours => OldInvoiceLines.Sum(x => x.BookingHours);
        public virtual double OldBookingHoursUnbillable => OldInvoiceLines.Sum(x => x.BookingHoursUnbillable);
        public virtual decimal OldPriceTotal => OldInvoiceLines.Sum(x => x.Price);
        public virtual double OldHoursMargin => OldHours - OldBookingHours >= 0 ? OldHours - OldBookingHours : 0;
        public virtual double ExtraHours => BookingHours - OldHoursMargin;
    }

    public class Correction : ModelBase
    {
        public virtual Invoice Invoice { get; set; }
        public virtual string Description { get; set; }
        public virtual decimal Price { get; set; }
    }
}
