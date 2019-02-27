namespace Gitle.Model.Mapping
{
    using FluentNHibernate.Mapping;

    public class CommentMap : TouchableMap<Comment>
    {
         public CommentMap()
         {
             Map(x => x.Text).CustomSqlType("nvarchar(max)").CustomType("StringClob");
             Map(x => x.CreatedAt);
             Map(x => x.IsInternal);
             References(x => x.User).Column("User_id").LazyLoad(Laziness.False);
             References(x => x.Issue).Column("Issue_id").LazyLoad(Laziness.False);
         }
    }
}