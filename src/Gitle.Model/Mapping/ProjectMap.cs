namespace Gitle.Model.Mapping
{
    public class ProjectMap : ModelBaseMap<Project>
    {
         public ProjectMap()
         {
             Map(x => x.Name);
             Map(x => x.Slug);
             Map(x => x.Repository);
             Map(x => x.MilestoneId);
             Map(x => x.MilestoneName);
             Map(x => x.HourPrice);
             Map(x => x.FreckleId);
             HasMany(x => x.Users).Cascade.All();
         }
    }
}