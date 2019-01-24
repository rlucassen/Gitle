namespace Gitle.Model
{
    using System;

    public class Setting : ModelBase
    {
        public virtual DateTime? ClosedForBookingsBefore { get; set; }
    }
}