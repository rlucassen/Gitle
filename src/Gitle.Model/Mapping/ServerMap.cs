namespace Gitle.Model.Mapping
{
    public class ServerMap : ModelBaseMap<Server>
    {
        public ServerMap()
        {
            Map(x => x.Name);
            Map(x => x.Slug);
            Map(x => x.HostingCompany);
            Map(x => x.IsExternal);
            HasMany(x => x.Contacts); //Mogelijk dat de contacts niet in het systeem zitten?
            HasMany(x => x.Installations);
            Map(x => x.Description).CustomSqlType("nvarchar(max)");
        }
    }
}