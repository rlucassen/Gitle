namespace Gitle.Model.Mapping
{
    public class InstallationMap : ModelBaseMap<Installation>
    {
        public InstallationMap()
        {
            Map(x => x.Slug);
            Map(x => x.InstallationType);
            Map(x => x.Url);
            Map(x => x.Description).CustomSqlType("nvarchar(max)");

            References(x => x.Server);
            References(x => x.Customer);
            References(x => x.Application);
        }
    }
}