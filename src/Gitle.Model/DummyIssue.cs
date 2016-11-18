using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gitle.Model
{
    using Enum;
    using Helpers;
    using Interfaces.Model;

    public class DummyIssue
    {
        public DummyIssue()
        {
            Bookings = new List<Booking>();
        }
        public virtual string Name { get; set; }
        public virtual IList<Booking> Bookings { get; set; }

        public virtual double BillableBookingHours()
        {
            return Bookings.Where(x => x.IsActive && !x.Unbillable).Sum(x => x.Hours);
        }

        public virtual string BillableBookingHoursString()
        {
            return BillableBookingHours().ToHourDayNotation();
        }

        public virtual double UnbillableBookingHours()
        {
            return Bookings.Where(x => x.IsActive && x.Unbillable).Sum(x => x.Hours);
        }

        public virtual string UnbillableBookingHoursString()
        {
            return UnbillableBookingHours().ToHourDayNotation();
        }
    }
}
