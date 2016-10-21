namespace Gitle.Model.Mapping
{
    public class PlanningResourceMap : ModelBaseMap<PlanningResource>
    {
        public PlanningResourceMap()
        {
            Map(x => x.Year);
            Map(x => x.Week);
            References(x => x.Project);
            HasManyToMany(x => x.Issues).Table("PlanningResourceIssue");
        }
    }
}