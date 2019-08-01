namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class HandOverMap : TouchableMap<HandOver>
    {
        public HandOverMap()
         {
             Map(x => x.CreatedAt);
             References(x => x.User).Column("User_id").LazyLoad(Laziness.False);
             References(x => x.ByUser).Column("ByUser_id").LazyLoad(Laziness.False);
             References(x => x.Issue).Column("Issue_id").LazyLoad(Laziness.False);
         }
    }
}