namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class UserProjectMap : ModelBaseMap<UserProject>
    {
        public UserProjectMap()
        {
            References(x => x.Project).Column("Project_id").LazyLoad(Laziness.False);
            References(x => x.User).Column("User_id").LazyLoad(Laziness.False);
            Map(x => x.Notifications);
            Map(x => x.OnlyOwnIssues);
            Map(x => x.ConfirmOwnEntries);
        }
    }
}