namespace Gitle.Model.Mapping
{
    public class PlanningCommentMap : ModelBaseMap<PlanningComment>
    {
        public PlanningCommentMap()
        {
            Map(x => x.Slug);
            Map(x => x.Comment).CustomSqlType("nvarchar(max");
            References(x => x.User);
        }
    }
}