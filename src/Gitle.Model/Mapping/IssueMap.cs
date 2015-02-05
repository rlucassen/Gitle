namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class IssueMap : ModelBaseMap<Issue>
    {
         public IssueMap()
         {
             Map(x => x.Number);
             Map(x => x.Name);
             Map(x => x.State);
             Map(x => x.Body).CustomSqlType("nvarchar(max)");
             Map(x => x.Hours);
             Map(x => x.Devvers);
             Map(x => x.CreatedAt);
             Map(x => x.ClosedAt);
             Map(x => x.UpdatedAt);
             References(x => x.User).Column("User_id");
             References(x => x.Project).Column("Project_id").LazyLoad(Laziness.False);
             HasMany(x => x.Comments).Cascade.AllDeleteOrphan();
             HasManyToMany(x => x.Labels).Table("IssueLabel").ParentKeyColumn("Issue_id").ChildKeyColumn("Label_id").Cascade.SaveUpdate();
         }
    }
}