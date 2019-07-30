namespace Gitle.Model.Mapping
{
    public class HostingMap : ModelBaseMap<Hosting>
    {
        public HostingMap()
        {
            Map(x => x.Description).CustomSqlType("nvarchar(max)");
            Map(x => x.BillingMail);
            Map(x => x.Name);
            Map(x => x.Phone);
            Map(x => x.Slug);
            Map(x => x.SupportMail);
            Map(x => x.Website);
            HasMany(x => x.Contacts);
        }
    }
}