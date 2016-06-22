namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class InvoiceLineMap : ModelBaseMap<InvoiceLine>
    {
        public InvoiceLineMap()
        {
            Map(x => x.Description).CustomSqlType("nvarchar(500)");
            Map(x => x.Hours);
            Map(x => x.Null).Column("[Null]");

            References(x => x.Invoice).Column("Invoice_id");
            References(x => x.Issue).Column("Issue_id");

            HasManyToMany(x => x.Bookings).Table("InvoiceLineBooking").Cascade.None();
        }
    }
}