namespace Gitle.Model.Mapping
{
    public class CustomerMap : ModelBaseMap<Customer>
    {
        public CustomerMap()
        {
            Map(x => x.Name);
            Map(x => x.Slug);
            Map(x => x.Comments).CustomSqlType("nvarchar(max)");
            HasMany(x => x.Projects);
            HasMany(x => x.Contacts);
            HasMany(x => x.Applications);
        }
    }
}