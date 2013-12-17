namespace Gitle.Model.Mapping
{
    public class ProjectMap : ModelBaseMap<Project>
    {
         public ProjectMap()
         {
             Map(x => x.Name);
             Map(x => x.Repository);
             Map(x => x.Milestone);
             References(x => x.Customer).Column("Customer_id");
         }
    }
}