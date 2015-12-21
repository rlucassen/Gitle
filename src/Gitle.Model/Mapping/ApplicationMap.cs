namespace Gitle.Model.Mapping
{
    public class ApplicationMap : ModelBaseMap<Application>
    {
        public ApplicationMap ()
        {
            Map(x => x.Name);
            Map(x => x.Slug);
            References(x => x.Customer);
            HasMany<Project>(x => x.Project);
        }
    }
}