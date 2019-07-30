namespace Gitle.Model.Mapping
{
    public class ServerMap : ModelBaseMap<Server>
    {
        public ServerMap()
        {
            Map(x => x.Name);
            Map(x => x.Slug); 
            Map(x => x.HaveAccessToServer);
            References(x => x.Hosting);
            HasMany(x => x.Installations);
            Map(x => x.Description).CustomSqlType("nvarchar(max)");
        }
    }
}