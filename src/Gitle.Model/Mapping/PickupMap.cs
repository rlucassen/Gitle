namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class PickupMap : TouchableMap<Pickup>
    {
        public PickupMap()
         {
             Map(x => x.CreatedAt);
             References(x => x.User).Column("User_id").LazyLoad(Laziness.False);
             References(x => x.Issue).Column("Issue_id").LazyLoad(Laziness.False);
         }
    }
}