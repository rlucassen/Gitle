using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitle.Model
{
    public class InvoiceLine : ModelBase
    {
        public virtual Invoice Invoice { get; set; }
        public virtual Issue Issue { get; set; }
        public virtual string Description { get; set; }
        public virtual double Hours { get; set; }
        public virtual double Price { get { return Hours * Invoice.HourPrice; } }
    }

    public class Correction : ModelBase
    {
        public virtual Invoice Invoice { get; set; }
        public virtual string Description { get; set; }
        public virtual double Price { get; set; }
    }
}
