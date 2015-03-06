namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class InvoiceMap : ModelBaseMap<Invoice>
    {
        public InvoiceMap()
        {
            Map(x => x.CreatedAt);
            Map(x => x.HourPrice);
            Map(x => x.Title);
            Map(x => x.Number);
            Map(x => x.VAT);
            Map(x => x.Remarks);

            References(x => x.Project).Column("Project_id");
            References(x => x.CreatedBy).Column("CreatedBy_id");

            HasMany(x => x.Lines).Cascade.All();
            HasMany(x => x.Corrections).Cascade.All();
            HasManyToMany(x => x.Bookings).Table("InvoiceBooking").ParentKeyColumn("Invoice_id").ChildKeyColumn("Booking_id").Cascade.SaveUpdate();
        }
    }
}