namespace Gitle.Model.Mapping
{
    public class CustomerMap : ModelBaseMap<Customer>
    {
        public CustomerMap()
        {
            Map(x => x.Name);
            Map(x => x.Slug);
            HasMany(x => x.Projects);
            HasMany(x => x.Contacts);
        }
    }
}