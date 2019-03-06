namespace Gitle.Model.Mapping
{
    public class InstallationMap : ModelBaseMap<Installation>
    {
        public InstallationMap()
        {
            Map(x => x.Name);
            Map(x => x.Slug);
            Map(x => x.Customer);
            Map(x => x.Application);
            Map(x => x.InstallationType);
            Map(x => x.Server);
            Map(x => x.Description).CustomSqlType("nvarchar(max)");
        }
    }
}